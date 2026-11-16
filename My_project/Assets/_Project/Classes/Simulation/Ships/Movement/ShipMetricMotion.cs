using UnityEngine;
using _Project.Scripts.Ships;

namespace _Project.Scripts.Simulation.Ships.Movement
{
    /// <summary>Общий метрический шаг для исполнения движения и прогноза; задачи и ИИ не изменяет.</summary>
    public static class ShipMetricMotion
    {
        private static readonly CourseChanger Course = new CourseChanger();
        private static readonly SpeedChanger Speed = new SpeedChanger();

        public static bool HasArrived(in Ship ship, in MoveToPointParams move)
        {
            return Vector3.Distance(ship.Position, move.Destination) <= move.Tolerance;
        }

        /// <summary>Меняет только позицию, ориентацию и скорость переданного корабля.</summary>
        public static void Step(ref Ship ship, in MoveToPointParams move, float deltaTime)
        {
            if (deltaTime <= 0f || HasArrived(in ship, in move))
                return; // Прибытие завершает исполнитель задачи, а не расчёт физики.

            var forward = ship.Rotation * Vector3.up;
            forward = forward.sqrMagnitude > 0f ? forward.normalized : Vector3.up;
            var direction = Course.GetDirection(ship.Position, forward, move.Destination, ship.Stats.Agility, deltaTime);
            float speed = Speed.GetSpeed(ref ship, in move, deltaTime);
            float distance = Vector3.Distance(ship.Position, move.Destination);
            if (direction.sqrMagnitude > 0f)
            {
                ship.Position += direction.normalized * Mathf.Min(speed * deltaTime, distance);
                ship.Rotation = Quaternion.LookRotation(Vector3.forward, direction);
            }
        }
    }
}
