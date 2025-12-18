using UnityEngine;
using _Project.Scripts.Const;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.Ships;

namespace _Project.Scripts.Simulation.Local.Stages.Movement
{
    /// <summary>Отвечает за расчёт целевой скорости и её изменение.</summary>
    public sealed class SpeedChanger
    {
        public float GetSpeed(ref Ship ship, in MoveToPointParams taskParams, float deltaTime)
        {
            if (deltaTime <= 0f)
                return ship.CurrentSpeed; // нет времени — скорость не меняем

            float maxSpeed = Mathf.Max(0f, ship.Stats.MaxSpeed);   // физический потолок скорости
            float agility = Mathf.Max(0f, ship.Stats.Agility);     // манёвренность = ускорение/торможение

            var toDest = taskParams.Destination - ship.Position;   // вектор до цели
            float distance = toDest.magnitude;                     // расстояние до цели
            float tolerance = taskParams.Tolerance;                // допускаемая зона

            float targetSpeed;
            if (taskParams.KeepSpeed || agility <= 0f)
            {
                targetSpeed = maxSpeed; // летим на максимум, не тормозим
            }
            else
            {
                // скорость, при которой успеем затормозить к границе толеранса
                float required = Mathf.Sqrt(Mathf.Max(0f, 2f * agility * (distance - tolerance)));
                targetSpeed = Mathf.Min(maxSpeed, required);

                // корректировка: если цель сильно сбоку, сужаем конус и снижаем скорость, чтобы довернуть
                var forward = ship.Rotation * Vector3.up;
                if (forward.sqrMagnitude > 0f && distance > 0f)
                {
                    forward.Normalize();
                    var desired = toDest / distance;
                    float angle = Vector3.Angle(forward, desired); // угол между носом и желаемым направлением
                    float allowed = agility * SimulationConsts.AgilityTurnConeFactor; // ширина допустимого конуса
                    if (allowed > 0f && angle > allowed)
                    {
                        float scale = Mathf.Clamp01(allowed / angle); // чем больше угол, тем сильнее режем скорость
                        targetSpeed *= scale;
                    }
                }
            }

            // плавно двигаем текущую скорость к целевой с учётом ускорения (манёвренность * коэффициент)
            float newSpeed = Mathf.MoveTowards(Mathf.Max(0f, ship.CurrentSpeed), targetSpeed, agility * SimulationConsts.AccelerationOfAgility * deltaTime);
            ship.CurrentSpeed = newSpeed; // применяем к кораблю
            return newSpeed;
        }
    }
}
