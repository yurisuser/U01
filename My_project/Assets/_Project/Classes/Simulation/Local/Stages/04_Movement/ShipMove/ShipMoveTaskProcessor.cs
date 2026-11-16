using UnityEngine;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.Ships;
using _Project.Scripts.Simulation.AI;
using _Project.Scripts.Simulation.Ships.Movement;

namespace _Project.Scripts.Simulation.Local.Stages.Movement
{
    /// <summary>Исполнение задачи MoveToPoint на один тик симуляции.</summary>
    internal sealed class ShipMoveTaskProcessor
    {
        public void ProcessMove(ref Ship ship, float deltaTime)
        {
            if (ProcessNewAiMove(ref ship, deltaTime))
                return;
            if (ship.Ai?.TaskExecution != null)
                return; // Другие задачи нового ИИ не должны запускать старый стек движения.

            if (!ship.TaskState.TryPeek(out var task) || task.Type != EShipTaskType.MoveToPoint)
                return; // На вершине стека не MoveToPoint.

            var moveTaskParams = task.Params.MoveToPointParams;
            if (ShipMetricMotion.HasArrived(in ship, in moveTaskParams))
            {
                CompleteTask(ref ship, in moveTaskParams); // Уже в зоне допуска.
                return;
            }

            ShipMetricMotion.Step(ref ship, in moveTaskParams, deltaTime);

            if (Vector3.Distance(ship.Position, moveTaskParams.Destination) <= moveTaskParams.Tolerance)
                CompleteTask(ref ship, in moveTaskParams); // Дошли до цели на этом тике.
        }

        private bool ProcessNewAiMove(ref Ship ship, float deltaTime)
        {
            var execution = ship.Ai?.TaskExecution;
            if (execution == null || !(execution.Task is FlyToPointTask task) || execution.IsFinished ||
                execution.Status == EShipAiTaskStatus.Suspended)
                return false;

            execution.Start();
            var currentPosition = ship.Position;
            var toTarget = task.Destination - currentPosition;
            float distance = toTarget.magnitude;
            if (distance <= task.Tolerance)
            {
                CompleteNewAiTask(ref ship, task, execution);
                return true;
            }

            var moveParams = new MoveToPointParams
            {
                Destination = task.Destination,
                Tolerance = task.Tolerance,
                KeepSpeed = task.KeepSpeed,
            };
            ShipMetricMotion.Step(ref ship, in moveParams, deltaTime);

            if (Vector3.Distance(ship.Position, task.Destination) <= task.Tolerance)
                CompleteNewAiTask(ref ship, task, execution);

            return true;
        }

        private static void CompleteNewAiTask(ref Ship ship, FlyToPointTask task, ShipAiTaskExecution execution)
        {
            if (!task.KeepSpeed)
                ship.CurrentSpeed = 0f;

            execution.Complete(EShipAiTaskOutcome.Succeeded);
        }

        private static void CompleteTask(ref Ship ship, in MoveToPointParams moveTaskParams)
        {
            if (!moveTaskParams.KeepSpeed)
                ship.CurrentSpeed = 0f; // Полная остановка после достижения точки.

            ship.TaskState.Pop(); // Удаляем выполненную задачу из стека.
        }
    }
}
