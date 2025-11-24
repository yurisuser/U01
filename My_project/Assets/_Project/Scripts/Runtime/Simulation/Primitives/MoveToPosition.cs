using _Project.Scripts.Simulation;
using _Project.Scripts.Ships;
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
            if (TrySnapToTarget(ref ship, target)) // если уже в радиусе прибытия — телепортируем и выходим
                return true; // цель достигнута мгновенно

            var forward = NormalizeDirectionShip(ship); // нормализованное направление корабля
            float maxTurnPerStep = GetMaxTurnPerStep(ship); // макс угол поворота за шаг в радианах

            var toTarget = target - ship.Position; // вектор до цели
            var distance = toTarget.magnitude;     // расстояние до цели
            var desiredDir = distance > Mathf.Epsilon ? toTarget / distance : Vector3.zero; // нормализованное направление к цели

            if (desiredDir.sqrMagnitude > Mathf.Epsilon && maxTurnPerStep > 0f) // есть куда поворачивать и ограничение не нулевое
            {
                forward = Vector3.RotateTowards(forward, desiredDir, maxTurnPerStep, 0f).normalized; // плавно поворачиваем нос к цели
            }

            float moveDistance = Mathf.Max(0f, ship.Stats.MaxSpeed); // сколько пройти за шаг по скорости
            if (desiredDir.sqrMagnitude > Mathf.Epsilon)
            {
                float distanceAlongForward = Vector3.Dot(toTarget, forward); // проекция цели на текущий курс
                if (distanceAlongForward <= 0f)
                    moveDistance = 0f;
                else if (moveDistance > distanceAlongForward)
                    moveDistance = distanceAlongForward; // не перелетаем цель
            }

            ship.Position += forward * moveDistance; // перемещаем корабль в сторону цели

            FinalizeOrientation(ref ship, forward); // обновляем Rotation по новому forward

            if (IsReached(ship.Position, target, SimulationConsts.ArriveDistance)) // проверяем достижение цели
            {
                return true; // цель достигнута
            }

            return false; // сообщаем, что остались на пути
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
            return Mathf.Max(0f, ship.Stats.Agility);
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
    }
}
