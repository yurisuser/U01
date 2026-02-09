using UnityEngine;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.Ships;

namespace _Project.Scripts.Simulation.Local.Stages.Movement
{
    /// <summary>Исполнение задачи MoveToPoint на один тик симуляции.</summary>
    internal sealed class ShipMoveTaskProcessor
    {
        private readonly CourseChanger _courseChanger;
        private readonly MoveChanger _moveChanger;
        private readonly SpeedChanger _speedChanger;

        public ShipMoveTaskProcessor(CourseChanger courseChanger, MoveChanger moveChanger, SpeedChanger speedChanger)
        {
            _courseChanger = courseChanger;
            _moveChanger = moveChanger;
            _speedChanger = speedChanger;
        }

        public void ProcessMove(ref Ship ship, float deltaTime)
        {
            if (!ship.TaskState.TryPeek(out var task) || task.Type != EShipTaskType.MoveToPoint)
                return; // На вершине стека не MoveToPoint.

            var moveTaskParams = task.Params.MoveToPointParams;
            var currentPosition = ship.Position;
            var currentDirection = GetCurrentDirection(in ship);

            var toTarget = moveTaskParams.Destination - currentPosition;
            float distance = toTarget.magnitude;
            if (distance <= moveTaskParams.Tolerance)
            {
                CompleteTask(ref ship, in moveTaskParams); // Уже в зоне допуска.
                return;
            }

            var nextDirection = _courseChanger.GetDirection(
                currentPosition,
                currentDirection,
                moveTaskParams.Destination,
                ship.Stats.Agility,
                deltaTime);
            var nextSpeed = _speedChanger.GetSpeed(ref ship, moveTaskParams, deltaTime);
            var stepShift = GetStepShift(nextDirection, nextSpeed, deltaTime, distance);
            var nextPosition = _moveChanger.GetShift(in ship, stepShift);

            Apply(ref ship, nextPosition, nextDirection, nextSpeed); // Применяем только после полного расчета шага.

            if (Vector3.Distance(ship.Position, moveTaskParams.Destination) <= moveTaskParams.Tolerance)
                CompleteTask(ref ship, in moveTaskParams); // Дошли до цели на этом тике.
        }

        private static Vector3 GetCurrentDirection(in Ship ship)
        {
            var forward = ship.Rotation * Vector3.up;
            return forward.sqrMagnitude > 0f ? forward.normalized : Vector3.up;
        }

        private static Vector3 GetStepShift(Vector3 direction, float speed, float deltaTime, float distance)
        {
            if (direction.sqrMagnitude <= 0f || speed <= 0f || deltaTime <= 0f)
                return Vector3.zero; // Нет валидного шага.

            float step = Mathf.Min(speed * deltaTime, distance);
            return direction.normalized * step;
        }

        private static void Apply(ref Ship ship, in Vector3 nextPosition, in Vector3 nextDirection, float nextSpeed)
        {
            ship.Position = nextPosition;
            ship.CurrentSpeed = nextSpeed;
            if (nextDirection.sqrMagnitude > 0f)
                ship.Rotation = Quaternion.LookRotation(Vector3.forward, nextDirection);
        }

        private static void CompleteTask(ref Ship ship, in MoveToPointParams moveTaskParams)
        {
            if (!moveTaskParams.KeepSpeed)
                ship.CurrentSpeed = 0f; // Полная остановка после достижения точки.

            ship.TaskState.Pop(); // Удаляем выполненную задачу из стека.
        }
    }
}
