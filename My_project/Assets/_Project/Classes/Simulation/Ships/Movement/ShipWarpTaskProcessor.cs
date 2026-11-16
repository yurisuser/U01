using UnityEngine;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.AI;

namespace _Project.Scripts.Simulation.Ships.Movement
{
    /// <summary>Исполняет задачу варпа и возвращает её результат; выбор маршрута остаётся у поведения.</summary>
    public static class ShipWarpTaskProcessor
    {
        public static void BeginTurn(ref Ship ship)
        {
            var execution = ship.Ai?.TaskExecution;
            if (execution?.Task is WarpToPointTask task && execution.Status == EShipAiTaskStatus.Running
                && ship.Warp.Phase == EShipWarpPhase.Metric && ship.Warp.HasWarpDestination
                && Vector3.Distance(ship.Position, task.Destination) <= task.ArrivalTolerance)
            {
                ship.Warp = default;
                execution.Complete(EShipAiTaskOutcome.Failed);
                return; // Зона назначения достигнута при подготовке: новый заряд уже не нужен.
            }

            ShipWarpProcessor.BeginTurn(ref ship);
            if (execution?.Task is WarpToPointTask && execution.Status == EShipAiTaskStatus.Running
                && ship.Warp.Phase == EShipWarpPhase.Metric && !ship.Warp.HasWarpDestination)
                execution.Complete(EShipAiTaskOutcome.Succeeded); // Результат торможения доступен ИИ уже на этой границе хода.
        }

        /// <summary>Возвращает true, если движение принадлежит задаче или заблокированной фазе варпа.</summary>
        public static bool Process(ref Ship ship, float deltaTime, bool isTurnStart)
        {
            var execution = ship.Ai?.TaskExecution;
            if (!(execution?.Task is WarpToPointTask task) || execution.IsFinished || execution.Status == EShipAiTaskStatus.Suspended)
                return ShipWarpProcessor.Process(ref ship, deltaTime, false); // Существующий физический варп продолжается независимо от новых задач.

            var arrival = new MoveToPointParams { Destination = task.Destination, Tolerance = task.ArrivalTolerance, KeepSpeed = true };
            if (execution.Status == EShipAiTaskStatus.Pending)
            {
                if (!isTurnStart)
                    return true; // Запрос принимаем только в начале игрового хода.
                float distanceSquared = (task.Destination - ship.Position).sqrMagnitude;
                if (float.IsNaN(distanceSquared) || float.IsInfinity(distanceSquared)
                    || float.IsNaN(task.ArrivalTolerance) || float.IsInfinity(task.ArrivalTolerance) || task.ArrivalTolerance < 0f
                    || ShipSpeed.GetMetricMaxSpeed(in ship) <= 0f || ShipSpeed.GetWarpSpeed(in ship) <= 0f
                    || ShipMetricMotion.HasArrived(in ship, in arrival)
                    || !ShipWarpProcessor.TryRequestWarp(ref ship, task.Destination))
                {
                    execution.Complete(EShipAiTaskOutcome.Rejected);
                    return true;
                }
                execution.Start();
                ShipWarpProcessor.BeginTurn(ref ship); // Новый запрос может сразу начать заряд на этой же границе.
            }

            if (ShipWarpProcessor.Process(ref ship, deltaTime, false))
                return true; // Заряд, варп и торможение полностью исполняет общий процессор фаз.

            if (ShipMetricMotion.HasArrived(in ship, in arrival)
                || isTurnStart && ShipWarpProcessor.IsAlignedAtMaxSpeed(in ship, task.Destination))
            {
                ship.Warp = default; // Подготовка потеряла смысл до начала заряда; активные фазы здесь никогда не отменяются.
                execution.Complete(EShipAiTaskOutcome.Failed);
                return true;
            }

            arrival.Tolerance = 0f;
            ShipMetricMotion.Step(ref ship, in arrival, deltaTime); // Выравниваем курс и набираем метрический максимум без торможения у цели.
            return true;
        }
    }
}
