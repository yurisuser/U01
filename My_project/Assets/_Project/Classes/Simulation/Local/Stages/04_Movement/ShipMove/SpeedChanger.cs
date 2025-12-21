using UnityEngine;
using _Project.Scripts.Const;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.Ships;

namespace _Project.Scripts.Simulation.Local.Stages.Movement
{
    /// <summary>Отвечает за расчёт целевой скорости и её изменение.</summary>
    /// <summary>
    /// 1. Определяем 
    /// </summary>
    public sealed class SpeedChanger
    {
        private static float CurrSpeed;
        private static float NextSpeed;
        private static float MaxSpeed;
        private static float Agility;
        private static float TargetSpeed;

        public float GetSpeed(ref Ship ship, in MoveToPointParams taskParams, float deltaTime)
        {
            if (deltaTime <= 0f)
                return ship.CurrentSpeed; // нет времени — скорость не меняем

            ClearLocalVariables();

            CurrSpeed = Mathf.Max(0f, ship.CurrentSpeed);
            MaxSpeed = GetMaxSpeed(in ship);   // физический потолок скорости
            Agility = GetAgility(in ship);     // манёвренность = ускорение/торможение

            TargetSpeed = GetTargetSpeed(ref ship, in taskParams, MaxSpeed, Agility);
            TargetSpeed = AdjustSpeedByTurnPrediction(ref ship, in taskParams, TargetSpeed);

            // плавно двигаем текущую скорость к целевой с учётом ускорения (манёвренность * коэффициент)
            NextSpeed = ApplySpeed(ref ship, TargetSpeed, Agility, deltaTime);
            return NextSpeed;
        }

        private static void ClearLocalVariables()
        {
            CurrSpeed = 0f;
            NextSpeed = 0f;
            MaxSpeed = 0f;
            Agility = 0f;
            TargetSpeed = 0f;
        }

        private static float GetMaxSpeed(in Ship ship)
        {
            return Mathf.Max(0f, ship.Stats.MaxSpeed);
        }

        private static float GetAgility(in Ship ship)
        {
            return Mathf.Max(0f, ship.Stats.Agility);
        }

        private static float GetTargetSpeed(ref Ship ship, in MoveToPointParams taskParams, float maxSpeed, float agility)
        {
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

            return targetSpeed;
        }

        private static float AdjustSpeedByTurnPrediction(ref Ship ship, in MoveToPointParams taskParams, float targetSpeed)
        {
            var toDest = taskParams.Destination - ship.Position;
            if (toDest.sqrMagnitude <= 0f)
                return targetSpeed;

            float distance = toDest.magnitude;
            float omega = Mathf.Max(0f, ship.Stats.Agility);
            if (omega <= 0f)
                return targetSpeed;

            float effectiveDistance = Mathf.Max(0f, distance - taskParams.Tolerance);
            if (effectiveDistance <= 0f)
                return targetSpeed;

            var velocityDir = ship.Rotation * Vector3.up;
            if (velocityDir.sqrMagnitude <= 0f)
                return targetSpeed;

            velocityDir.Normalize();
            var toDestDir = toDest / distance;

            float angleRad = Mathf.Acos(Mathf.Clamp(Vector3.Dot(velocityDir, toDestDir), -1f, 1f));
            if (angleRad <= 0.0001f)
                return targetSpeed;

            float maxAllowedSpeed = (omega * effectiveDistance) / angleRad;
            if (targetSpeed > maxAllowedSpeed)
                return Mathf.Max(0f, maxAllowedSpeed);

            return targetSpeed;
        }

        private static float ApplySpeed(ref Ship ship, float targetSpeed, float agility, float deltaTime)
        {
            float newSpeed = Mathf.MoveTowards(
                CurrSpeed,
                targetSpeed,
                agility * SimulationConsts.AccelerationOfAgility * deltaTime);
            ship.CurrentSpeed = newSpeed; // применяем к кораблю
            return newSpeed;
        }
    }
}
