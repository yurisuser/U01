using _Project.Scripts.Core;
using _Project.Scripts.Simulation;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.Render;
using UnityEngine;

namespace _Project.Scripts.Simulation.Primitives
{
    /// <summary>Примитив перемещения корабля к точке с трассировкой.</summary>
    internal static class MoveToPosition
    {
        /// <summary>
        /// Двигает корабль к целевой точке за один логический шаг, фиксируя трейсы и возвращая true,
        /// если за шаг достигли цели (или были в радиусе прибытия).
        /// </summary>
        public static bool Execute(ref Ship ship, // корабль, у которого обновляем позицию/скорость/ориентацию
                                    in Vector3 target, // целевая точка в мире
                                    Render.ITraceSink traceSink = null, // приёмник трейса движения (может быть null)
                                    UID traceUid = default) // идентификатор для записи трейса
        {
            if (TrySnapToTarget(ref ship, target)) // если уже в радиусе прибытия — телепортируем и выходим
                return true; // цель достигнута мгновенно

            var forward = ResolveForward(ship);         // нормализованный «нос» корабля
            float turnRadius = ResolveTurnRadius(ship); // радиус разворота по манёвренности

            var toTarget = target - ship.Position; // вектор до цели
            var distance = toTarget.magnitude;     // расстояние до цели
            var desiredDir = distance > Mathf.Epsilon ? toTarget / distance : Vector3.zero; // нормализованное направление к цели

            if (desiredDir.sqrMagnitude > Mathf.Epsilon && !float.IsInfinity(turnRadius)) // есть куда поворачивать и радиус конечный
            {
                float maxTurnRate = 1f / Mathf.Max(turnRadius, 0.0001f); // рад/шаг доступного поворота
                float maxTurn = maxTurnRate;                             // макс радиан за шаг
                forward = Vector3.RotateTowards(forward, desiredDir, maxTurn, 0f).normalized; // плавно поворачиваем нос к цели
            }

            ship.Position = target; // перемещаем корабль напрямую в точку цели

            if (traceSink != null && ship.Uid.Id != 0) // если нужен трейс и UID валиден
                traceSink.AddSample(in traceUid, 1f, in ship.Position, ship.Rotation); // пишем финальный сэмпл движения

            FinalizeOrientation(ref ship, forward); // обновляем Rotation по новому forward

            if (IsArrived(ship.Position, target, SimulationConsts.ArriveDistance)) // проверяем достижение цели
            {
                ship.Velocity = Vector3.zero; // гасим скорость на финише
                return true; // цель достигнута
            }

            ship.Velocity = Vector3.zero; // цель не достигнута, но тормозим
            return false; // сообщаем, что остались на пути
        }

        /// <summary>Мгновенно обнуляет скорость корабля.</summary>
        public static void Stop(ref Ship ship)
        {
            ship.Velocity = Vector3.zero;
        }

        // --- helpers ---

        /// <summary>Если уже почти у цели — телепортируемся и завершаем.</summary>
        private static bool TrySnapToTarget(ref Ship ship, in Vector3 target)
        {
            var toTarget = target - ship.Position; // вектор до цели
            var distance = toTarget.magnitude;     // расстояние до цели
            if (distance > SimulationConsts.ArriveDistance)
                return false;

            ship.Position = target;
            ship.Velocity = Vector3.zero;
            return true;
        }

        /// <summary>Возвращает нормализованный «нос» корабля по его вращению.</summary>
        private static Vector3 ResolveForward(in Ship ship)
        {
            var forward = ship.Rotation * Vector3.right; // локальный right в мировых координатах
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.right;
            return forward.normalized;
        }

        /// <summary>Считает радиус разворота из манёвренности (Agility).</summary>
        private static float ResolveTurnRadius(in Ship ship)
        {
            return ship.Stats.Agility > 0f ? 1f / ship.Stats.Agility : float.PositiveInfinity;
        }

        /// <summary>Обновляет ориентацию корабля по текущему «носу».</summary>
        private static void FinalizeOrientation(ref Ship ship, in Vector3 forward)
        {
            if (forward.sqrMagnitude <= Mathf.Epsilon)
                return;

            float angleDeg = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
            ship.Rotation = Quaternion.Euler(0f, 0f, angleDeg);
        }

        /// <summary>Проверяет, что pos достаточно близко к target по arriveDistance.</summary>
        private static bool IsArrived(in Vector3 pos, in Vector3 target, float arriveDistance)
        {
            var remaining = target - pos;
            return remaining.sqrMagnitude <= arriveDistance * arriveDistance;
        }
    }
}
