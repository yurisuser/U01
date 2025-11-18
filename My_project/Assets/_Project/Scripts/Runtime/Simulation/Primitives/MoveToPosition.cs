using _Project.Scripts.Core;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.Render;
using UnityEngine;

namespace _Project.Scripts.Simulation.Primitives
{
    internal static class MoveToPosition
    {
        private static SubstepTraceBuffer _trace; // буфер для записи сабстепов
        private static UID _traceUid;             // UID корабля, чей трейс пишем

        // назначает буфер записи сабстепов для текущего корабля
        public static void SetTraceWriter(Render.SubstepTraceBuffer trace, in UID uid)
        {
            _trace = trace;
            _traceUid = uid;
        }

        // сбрасывает буфер записи сабстепов
        public static void ClearTraceWriter()
        {
            _trace = null;
            _traceUid = default;
        }

        // движет корабль к цели; возвращает true, если цель достигнута в течение dt
        public static bool Execute(ref Ship ship, 
                                    in Vector3 target, 
                                    float desiredSpeed, 
                                    float arriveDistance, 
                                    float dt, 
                                    bool stopOnArrival = true)
        {
            PrepareInputs(ref ship, ref desiredSpeed, ref arriveDistance);

            if (TrySnapToTarget(ref ship, target, arriveDistance, stopOnArrival))
                return true;

            var forward = ResolveForward(ship);         // нормализованный «нос» корабля
            float turnRadius = ResolveTurnRadius(ship); // радиус разворота по манёвренности

            int steps = ComputeSubstepCount(dt); // сколько сабшагов выполнить за dt
            float subDt = dt / steps;            // длительность одного сабшага
            bool reachedTarget = false;          // флаг, что на одном из сабшагов дошли
            float accumulatedTime = 0f;          // сколько времени уже симулировано

            for (int i = 0; i < steps; i++)
            {
                if (StepSubframe(ref ship, ref forward, target, desiredSpeed, arriveDistance, subDt, dt, ref accumulatedTime, turnRadius))
                {
                    reachedTarget = true;
                    break;
                }
            }

            FinalizeOrientation(ref ship, forward);

            if (IsArrived(ship.Position, target, arriveDistance) || reachedTarget)
            {
                ship.Position = target;
                if (stopOnArrival)
                    ship.Velocity = Vector3.zero;
                return true;
            }

            ship.Velocity = forward * desiredSpeed;
            return false;
        }

        // мгновенно останавливает корабль
        public static void Stop(ref Ship ship)
        {
            ship.Velocity = Vector3.zero;
        }

        // --- helpers ---

        // нормализует входные данные: arriveDistance и желаемую скорость
        private static void PrepareInputs(ref Ship ship, ref float desiredSpeed, ref float arriveDistance)
        {
            arriveDistance = Mathf.Max(arriveDistance, 0.01f);

            desiredSpeed = Mathf.Max(desiredSpeed, 0.1f);
            if (ship.Stats.MaxSpeed > 0f)
                desiredSpeed = Mathf.Min(desiredSpeed, ship.Stats.MaxSpeed);
        }

        // если уже почти у цели — телепортируем и завершаем
        private static bool TrySnapToTarget(ref Ship ship, in Vector3 target, float arriveDistance, bool stopOnArrival)
        {
            var toTarget = target - ship.Position; // вектор до цели
            var distance = toTarget.magnitude;     // расстояние до цели
            if (distance > arriveDistance)
                return false;

            ship.Position = target;
            if (stopOnArrival)
                ship.Velocity = Vector3.zero;
            return true;
        }

        // вычисляет текущий «нос» корабля и нормализует его
        private static Vector3 ResolveForward(in Ship ship)
        {
            var forward = ship.Rotation * Vector3.right; // локальный right в мировых координатах
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.right;
            return forward.normalized;
        }

        // возвращает радиус разворота из манёвренности
        private static float ResolveTurnRadius(in Ship ship)
        {
            return ship.Stats.Agility > 0f ? 1f / ship.Stats.Agility : float.PositiveInfinity;
        }

        // считает количество сабшагов для заданного dt
        private static int ComputeSubstepCount(float dt)
        {
            const float MaxSubstep = 0.05f;
            return Mathf.Clamp(Mathf.CeilToInt(dt / MaxSubstep), 1, 60);
        }

        // выполняет один сабшаг движения; true, если цель достигнута
        private static bool StepSubframe(ref Ship ship,
                                         ref Vector3 forward,
                                         in Vector3 target,
                                         float desiredSpeed,
                                         float arriveDistance,
                                         float subDt,
                                         float totalDt,
                                         ref float accumulatedTime,
                                         float turnRadius)
        {
            var toTarget = target - ship.Position; // вектор до цели
            var distance = toTarget.magnitude;     // расстояние до цели

            if (distance <= arriveDistance)
            {
                ship.Position = target;
                return true;
            }

            var desiredDir = distance > Mathf.Epsilon ? toTarget / distance : Vector3.zero; // нормализованное направление к цели

            if (desiredDir.sqrMagnitude > Mathf.Epsilon && !float.IsInfinity(turnRadius))
            {
                float maxTurnRate = desiredSpeed / Mathf.Max(turnRadius, 0.0001f); // рад/с доступного поворота
                float maxTurn = maxTurnRate * subDt;                               // макс радиан за сабшаг
                forward = Vector3.RotateTowards(forward, desiredDir, maxTurn, 0f).normalized;
            }

            float subDistance = desiredSpeed * subDt; // сколько пройдём за сабшаг

            if (desiredDir.sqrMagnitude > Mathf.Epsilon)
            {
                float distanceAlongForward = Vector3.Dot(toTarget, forward); // проекция до цели на курс
                if (distanceAlongForward <= 0f)
                    return false;

                if (subDistance > distanceAlongForward)
                    subDistance = distanceAlongForward;
            }

            ship.Position += forward * subDistance;

            accumulatedTime += subDt;
            if (_trace != null && ship.Uid.Id != 0)
            {
                float tFrac = Mathf.Clamp01(accumulatedTime / totalDt); // нормированное время внутри dt
                _trace.AddSample(in _traceUid, tFrac, in ship.Position, ship.Rotation);
            }

            return false;
        }

        // обновляет ориентацию корабля по текущему «носу»
        private static void FinalizeOrientation(ref Ship ship, in Vector3 forward)
        {
            if (forward.sqrMagnitude <= Mathf.Epsilon)
                return;

            float angleDeg = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
            ship.Rotation = Quaternion.Euler(0f, 0f, angleDeg);
        }

        // проверяет, что точка pos достаточно близко к target
        private static bool IsArrived(in Vector3 pos, in Vector3 target, float arriveDistance)
        {
            var remaining = target - pos;
            return remaining.sqrMagnitude <= arriveDistance * arriveDistance;
        }
    }
}
