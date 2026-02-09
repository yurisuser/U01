using System.Collections.Generic;
using _Project.Scripts.Ships;
using _Project.Scripts.Const;
using _Project.Scripts.Simulation.Continuum;
using _Project.Scripts.Simulation.Core;
using _Project.Scripts.Simulation.Ships;
using UnityEngine;

namespace _Project.Scripts.Simulation.Global.Stages.Movement
{
    /// <summary>Глобальное дискретное перемещение: один шаг задачи движения за ход.</summary>
    public sealed class GlobalMovementStage : ISimulationStage
    {
        public void Run(in SimulationStepContext context)
        {
            var gameState = context.GameState;
            var galaxy = gameState?.Galaxy;
            if (gameState == null || galaxy == null || galaxy.Length == 0)
                return; // Нет данных для глобального движения.

            int activeSystemIndex = context.ActiveSystemIndex;
            for (int systemIndex = 0; systemIndex < galaxy.Length; systemIndex++)
            {
                if (systemIndex == activeSystemIndex)
                    continue; // Активная система двигается локальным пайплайном.

                var system = galaxy[systemIndex];
                var runtime = system.State;
                if (runtime == null)
                    continue; // Нет кораблей для обработки.

                var ships = runtime.Ships;
                for (int i = 0; i < ships.Count; i++)
                {
                    var ship = ships[i];
                    if (TryProcessJump(ref ship, gameState, systemIndex, ships, i))
                    {
                        i--; // Компенсация RemoveAt при уходе в континуум.
                        continue;
                    }

                    ProcessMoveTo(ref ship, context.DeltaTime);
                    ships[i] = ship;
                }

                galaxy[systemIndex] = system;
            }
        }

        private static bool ProcessMoveTo(ref Ship ship, float deltaTime)
        {
            if (!ship.TaskState.TryPeek(out var task) || task.Type != EShipTaskType.MoveToPoint)
                return false; // Текущая задача не относится к перемещению.

            var move = task.Params.MoveToPointParams;
            var toTarget = move.Destination - ship.Position;
            float distance = toTarget.magnitude;
            if (distance <= move.Tolerance)
            {
                CompleteMoveTask(ref ship, in move);
                return true;
            }

            // В глобале двигаем дискретно на дистанцию, которую корабль проходит за ход.
            float stepSeconds = deltaTime > 0f ? deltaTime : SimulationConsts.GlobalStepSeconds;
            float speed = ship.CurrentSpeed > 0f ? ship.CurrentSpeed : Mathf.Max(0f, ship.Stats.MaxSpeed);
            if (speed <= 0f || stepSeconds <= 0f)
                return false; // Некорректные параметры движения: оставляем задачу на следующий ход.

            float stepDistance = speed * stepSeconds;
            if (stepDistance >= distance)
            {
                ship.Position = move.Destination;
                CompleteMoveTask(ref ship, in move);
                return true;
            }

            Vector3 dir = toTarget / distance;
            ship.Position += dir * stepDistance;
            ship.CurrentSpeed = speed;
            if (dir.sqrMagnitude > 0f)
                ship.Rotation = Quaternion.LookRotation(Vector3.forward, dir);

            return true;
        }

        private static void CompleteMoveTask(ref Ship ship, in MoveToPointParams move)
        {
            ship.Position = move.Destination;
            if (!move.KeepSpeed)
                ship.CurrentSpeed = 0f;
            ship.TaskState.Pop();
        }

        private static bool TryProcessJump(
            ref Ship ship,
            _Project.Scripts.Core.GameState.GameStateService gameState,
            int fromSystemIndex,
            List<Ship> ships,
            int shipIndex)
        {
            if (!ship.TaskState.TryPeek(out var task) || task.Type != EShipTaskType.JumpToSystem)
                return false; // Верхняя задача не прыжок.

            var service = ContinuumService.Instance;
            if (service == null)
                return false; // Сервис континуума не инициализирован.

            int toSystemIndex = task.Params.JumpToSystemParams.TargetSystemIndex;
            var galaxy = gameState.Galaxy;
            if (galaxy == null ||
                fromSystemIndex < 0 || fromSystemIndex >= galaxy.Length ||
                toSystemIndex < 0 || toSystemIndex >= galaxy.Length)
                return false; // Некорректные индексы систем.

            if (!service.TryGetZone(fromSystemIndex, toSystemIndex, out var zone))
                return false; // Между системами нет зоны перехода.

            float distance = Vector3.Distance(ship.Position, zone.Center);
            if (distance > zone.Radius)
                return false; // Корабль ещё не дошёл до зоны.

            ships.RemoveAt(shipIndex);
            var transit = service.CreateTransit(ship, fromSystemIndex, toSystemIndex, galaxy);
            service.Enqueue(in transit);
            ship.TaskState.Pop();
            return true;
        }
    }
}
