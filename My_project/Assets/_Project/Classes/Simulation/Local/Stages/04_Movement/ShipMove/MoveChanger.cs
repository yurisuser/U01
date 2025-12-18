using UnityEngine;
using _Project.Scripts.Simulation.Ships;
using _Project.Scripts.Ships;

namespace _Project.Scripts.Simulation.Local.Stages.Movement
{
    /// <summary>Отвечает за расчёт смещения и скорости корабля.</summary>
    public sealed class MoveChanger
    {
        public void UpdateMotion(ref Ship ship, in Vector3 direction, float deltaTime)
        {
            if (direction.sqrMagnitude <= 0f || deltaTime <= 0f)
                return;

            float maxSpeed = Mathf.Max(0f, ship.Stats.MaxSpeed);
            float accel = Mathf.Max(0f, ship.Stats.Agility);

            // пытаемся плавно выйти на нужную скорость
            float targetSpeed = maxSpeed;
            if (ship.TaskState.TryPeek(out var task))
            {
                var move = task.Params.MoveToPointParams;
                if (!move.KeepSpeed && accel > 0f)
                {
                    var toDest = move.Destination - ship.Position;
                    float distance = toDest.magnitude;
                    float tolerance = move.Tolerance;
                    float required = Mathf.Sqrt(Mathf.Max(0f, 2f * accel * (distance - tolerance)));
                    targetSpeed = Mathf.Min(maxSpeed, required);
                }
            }

            ship.CurrentSpeed = Mathf.MoveTowards(Mathf.Max(0f, ship.CurrentSpeed), targetSpeed, accel * deltaTime);

            if (ship.CurrentSpeed <= 0f)
                return;

            direction.Normalize();
            ship.Position += direction * (ship.CurrentSpeed * deltaTime);
        }
    }
}
