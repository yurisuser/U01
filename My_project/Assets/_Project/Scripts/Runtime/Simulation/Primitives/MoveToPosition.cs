using _Project.Scripts.Simulation;
using _Project.Scripts.Ships;
using _Project.Scripts.Galaxy.Config;
using UnityEngine;

namespace _Project.Scripts.Simulation.Primitives
{
    /// <summary>Примитив перемещения корабля к точке</summary>
    internal static class MoveToPosition
    {
        /// <summary>
        /// Двигает корабль к целевой точке за один логический шаг и возвращает true,
        /// если за шаг достигли цели (или были в радиусе прибытия).
        /// </summary>
        //-----------------------------------------------------------------------------------------------------
        public static bool Execute(ref Ship ship, in Vector3 target) 
        {
            ResetPath(ref ship);
            AppendSample(ref ship, ship.Position, ship.Rotation, 0f);

            var safeTarget = ApplyDeadZone(ship.Position, target); // корректируем цель с учётом мёртвых зон

            if (TrySnapToTarget(ref ship, safeTarget)) // если уже в радиусе прибытия — телепортируем и выходим
            {
                AppendSample(ref ship, ship.Position, ship.Rotation, 1f);
                return true; // цель достигнута мгновенно
            }

            var forward = NormalizeDirectionShip(ship); // нормализованное направление корабля
            float maxTurnPerStep = GetMaxTurnPerStep(ship); // макс угол поворота за весь шаг в радианах (ограничен)

            var toTarget = safeTarget - ship.Position; // вектор до цели
            var distance = toTarget.magnitude;     // расстояние до цели
            var desiredDir = distance > Mathf.Epsilon ? toTarget / distance : Vector3.zero; // нормализованное направление к цели

            float moveDistance = Mathf.Max(0f, Mathf.Min(ship.Stats.MaxSpeed, distance)); // сколько пройти за шаг по скорости, не перелетая
            int capacityLeft = Ship.PathSampleCapacity - ship.Path.Count;
            int steps = Mathf.Clamp(capacityLeft, 1, Ship.PathSampleCapacity);
            float perSubTurn = steps > 0 ? maxTurnPerStep / steps : 0f; // угол на подшаг, чтобы за весь шаг не превысить agility

            float subDistance = steps > 0 ? moveDistance / steps : moveDistance;
            bool reached = false;

            for (int i = 0; i < steps; i++)
            {
                if (desiredDir.sqrMagnitude > Mathf.Epsilon && perSubTurn > 0f)
                    forward = Vector3.RotateTowards(forward, desiredDir, perSubTurn, 0f).normalized;

                ship.Position += forward * subDistance;
                FinalizeOrientation(ref ship, forward);

                float t = (i + 1) / (float)steps;
                AppendSample(ref ship, ship.Position, ship.Rotation, t);

                if (IsReached(ship.Position, target, SimulationConsts.ArriveDistance))
                {
                    reached = true;
                    break;
                }
            }

            if (!reached && IsReached(ship.Position, safeTarget, SimulationConsts.ArriveDistance))
                reached = true;

            if (reached)
                return true; // цель достигнута

            return false; // Если не будет достигнута
        }
        //-----------------------------------------------------------------------------------------------------
        // --- helpers ---

        /// <summary>Если уже почти у цели — телепортируемся и завершаем.</summary>
        private static bool TrySnapToTarget(ref Ship ship, in Vector3 target)
        {
            var toTarget = target - ship.Position; // вектор до цели
            var distance = toTarget.magnitude;     // расстояние до цели
            if (distance > SimulationConsts.ArriveDistance)
                return false;

            ship.Position = target;
            return true;
        }
        //-----------------------------------------------------------------------------------------------------
        /// <summary>Возвращает нормализованный «нос» корабля по его вращению.</summary>
        private static Vector3 NormalizeDirectionShip(in Ship ship)
        {
            var forward = ship.Rotation * Vector3.right; // локальный Vector3.right в мировых координатах
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.right;
            return forward.normalized;
        }
        //-----------------------------------------------------------------------------------------------------
        /// <summary>Возвращает максимально доступный угол поворота за шаг из манёвренности (радианы).</summary>
        private static float GetMaxTurnPerStep(in Ship ship)
        {
            // Ограничиваем максимальный угол за весь шаг, чтобы не было "супер-резких" разворотов.
            const float MaxAngleRad = Mathf.PI / 2f; // не больше 90 град/шаг
            return Mathf.Clamp(ship.Stats.Agility, 0f, MaxAngleRad);
        }
        //-----------------------------------------------------------------------------------------------------
        /// <summary>Обновляет ориентацию корабля по текущему «носу».</summary>
        private static void FinalizeOrientation(ref Ship ship, in Vector3 forward)
        {
            if (forward.sqrMagnitude <= Mathf.Epsilon) return;
            ship.Rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg);
        }
        //-----------------------------------------------------------------------------------------------------
        /// <summary>Проверяет, что pos достаточно близко к target по arriveDistance.</summary>
        private static bool IsReached(in Vector3 pos, in Vector3 target, float arriveDistance)
        {
            var remaining = target - pos;
            return remaining.sqrMagnitude <= arriveDistance * arriveDistance;
        }

        private static Vector3 ApplyDeadZone(in Vector3 start, in Vector3 target)
        {
            // Радиусы мёртвых зон в юнитах сцены (орбиты вокруг (0,0))
            float orbitUnit = OrbitMath.PlanetOrbitIndexToUnits(1);
            float innerRadius = Mathf.Max(0f, SimulationConsts.InnerDeadZoneOrbits * orbitUnit);
            // Внешний радиус пока не обрабатываем

            var toTarget = target;
            float dist = toTarget.magnitude;

            // Если цель внутри — проецируем на границу
            if (dist < innerRadius && dist > 0.0001f)
            {
                return toTarget.normalized * innerRadius;
            }

            // Если старт внутри — выталкиваем на границу
            float startDist = start.magnitude;
            if (startDist < innerRadius && startDist > 0.0001f)
                return start.normalized * innerRadius;

            // Если отрезок пересекает круг — уходим на касательную к внутреннему радиусу
            if (SegmentIntersectsCircle(start, target, innerRadius))
                return ComputeTangentPoint(start, target, innerRadius);

            return target;
        }

        private static bool SegmentIntersectsCircle(in Vector3 a, in Vector3 b, float radius)
        {
            var d = b - a;
            float lenSq = d.sqrMagnitude;
            if (lenSq < Mathf.Epsilon)
                return a.magnitude < radius;

            float t = Mathf.Clamp01(Vector3.Dot(-a, d) / lenSq);
            var closest = a + d * t;
            return closest.sqrMagnitude < radius * radius;
        }

        private static Vector3 ComputeTangentPoint(in Vector3 start, in Vector3 target, float radius)
        {
            var fromCenter = start;
            float dist = fromCenter.magnitude;
            if (dist <= radius || dist < 0.0001f)
                return target; // fallback

            float angleToStart = Mathf.Atan2(fromCenter.y, fromCenter.x);
            float angleOffset = Mathf.Acos(Mathf.Clamp(radius / dist, -1f, 1f));

            // Выбор стороны обхода: используем знак z-компоненты cross(start, target)
            float cross = Mathf.Sign(fromCenter.x * target.y - fromCenter.y * target.x);
            float tangentAngle = angleToStart + angleOffset * (cross >= 0 ? 1f : -1f);

            return new Vector3(Mathf.Cos(tangentAngle) * radius, Mathf.Sin(tangentAngle) * radius, 0f);
        }

        private static void ResetPath(ref Ship ship)
        {
            ship.Path.Clear();
        }

        private static void AppendSample(ref Ship ship, in Vector3 pos, in Quaternion rot, float t)
        {
            ship.Path.TryAdd(new ShipPathSample
            {
                Position = pos,
                Rotation = rot,
                T = Mathf.Clamp01(t)
            });
        }
    }
}
