using UnityEngine;
using _Project.Scripts.Const;
using _Project.Scripts.Ships;

namespace _Project.Scripts.Simulation.Ships.Movement
{
    /// <summary>Исполняет фазы внутрисистемного варпа одинаково для локальной и глобальной симуляции.</summary>
    public static class ShipWarpProcessor
    {
        public static bool TryRequestWarp(ref Ship ship, in Vector3 destination)
        {
            if (ship.Warp.Phase != EShipWarpPhase.Metric || ship.Warp.HasWarpDestination)
                return false; // Новый приказ нельзя поставить во время активного или ожидающего варпа.

            ship.Warp.HasWarpDestination = true;
            ship.Warp.WarpDestination = destination;
            return true;
        }

        public static bool Process(ref Ship ship, float deltaTime, bool isTurnStart)
        {
            if (isTurnStart)
                BeginTurn(ref ship); // Переходы фаз разрешены только на границе игрового хода.

            switch (ship.Warp.Phase)
            {
                case EShipWarpPhase.Charging:
                case EShipWarpPhase.MetricBrake:
                    MoveOnLockedCourse(ref ship, ShipSpeed.GetMetricMaxSpeed(in ship), deltaTime);
                    return true;

                case EShipWarpPhase.Warp:
                    MoveInWarp(ref ship, deltaTime);
                    return true;

                default:
                    return false; // В свободной метрике управление остаётся у обычного обработчика движения.
            }
        }

        /// <summary>Применяет одну границу хода; пайплайн вызывает до принятия новых решений ИИ.</summary>
        public static void BeginTurn(ref Ship ship)
        {
            switch (ship.Warp.Phase)
            {
                case EShipWarpPhase.Metric:
                    TryStartCharge(ref ship);
                    break;

                case EShipWarpPhase.Charging:
                    ship.Warp.RemainingTurns--;
                    if (ship.Warp.RemainingTurns <= 0)
                    {
                        ship.Warp.Phase = EShipWarpPhase.Warp;
                        ship.CurrentSpeed = ShipSpeed.GetWarpSpeed(in ship);
                    }
                    break;

                case EShipWarpPhase.Warp:
                    if (HasReachedExitPoint(in ship))
                    {
                        ship.Warp.Phase = EShipWarpPhase.MetricBrake;
                        ship.Warp.RemainingTurns = SimulationConsts.MetricBrakeTurns;
                        ship.CurrentSpeed = ShipSpeed.GetMetricMaxSpeed(in ship);
                    }
                    break;

                case EShipWarpPhase.MetricBrake:
                    ship.Warp.RemainingTurns--;
                    if (ship.Warp.RemainingTurns <= 0)
                    {
                        ship.Warp = default; // Торможение завершено: очищаем назначение и возвращаем свободную метрику.
                        ship.CurrentSpeed = ShipSpeed.GetMetricMaxSpeed(in ship);
                    }
                    break;
            }
        }

        private static void TryStartCharge(ref Ship ship)
        {
            if (!ship.Warp.HasWarpDestination)
                return; // Нет приказа варпа, остаёмся в свободной метрике.

            var toDestination = ship.Warp.WarpDestination - ship.Position;
            float distance = toDestination.magnitude;
            float metricMaxSpeed = ShipSpeed.GetMetricMaxSpeed(in ship);
            float chargeDistance = metricMaxSpeed * SimulationConsts.GlobalStepSeconds * SimulationConsts.WarpChargeTurns;
            if (distance - chargeDistance - SimulationConsts.WarpExitRadiusMetric <= SimulationConsts.MovementPositionEpsilon)
                return; // За время заряда нельзя достичь или пролететь точку выхода.

            if (ShipSpeed.GetWarpSpeed(in ship) <= 0f)
                return; // Нулевая варповая скорость не позволяет выполнить перелёт.

            if (!IsAlignedAtMaxSpeed(in ship, ship.Warp.WarpDestination))
                return; // Скорость и курс ещё не соответствуют условиям заряда.

            ship.Warp.Phase = EShipWarpPhase.Charging;
            ship.Warp.LockedDirection = toDestination / distance;
            ship.Warp.RemainingTurns = SimulationConsts.WarpChargeTurns;
            ship.CurrentSpeed = metricMaxSpeed;
        }

        /// <summary>Общая проверка готовности скорости и курса для исполнения и прогноза.</summary>
        public static bool IsAlignedAtMaxSpeed(in Ship ship, in Vector3 destination)
        {
            float metricMaxSpeed = ShipSpeed.GetMetricMaxSpeed(in ship);
            var course = ship.Rotation * Vector3.up;
            return metricMaxSpeed > 0f && course.sqrMagnitude > 0f
                && Mathf.Abs(ship.CurrentSpeed - metricMaxSpeed) <= SimulationConsts.MovementSpeedEpsilon
                && Vector3.Angle(course, destination - ship.Position) <= SimulationConsts.WarpCourseToleranceDegrees;
        }

        private static void MoveOnLockedCourse(ref Ship ship, float speed, float deltaTime)
        {
            ship.CurrentSpeed = speed; // В заряде и торможении удерживаем метрический максимум.
            MoveAlongDirection(ref ship, ship.Warp.LockedDirection, speed, deltaTime);
        }

        private static void MoveInWarp(ref Ship ship, float deltaTime)
        {
            var exitPoint = GetExitPoint(in ship);
            var toExit = exitPoint - ship.Position;
            float distance = toExit.magnitude;
            if (distance <= SimulationConsts.MovementPositionEpsilon)
            {
                ship.Position = exitPoint; // До границы следующего хода остаёмся в точке автоматического выхода.
                return;
            }

            float speed = ShipSpeed.GetWarpSpeed(in ship);
            float step = Mathf.Max(0f, speed) * Mathf.Max(0f, deltaTime);
            if (step >= distance)
            {
                ship.Position = exitPoint; // Не пересекаем точку выхода: торможение начнётся на следующей границе хода.
                ship.CurrentSpeed = speed;
                return;
            }

            ship.CurrentSpeed = speed;
            MoveAlongDirection(ref ship, ship.Warp.LockedDirection, speed, deltaTime);
        }

        private static bool HasReachedExitPoint(in Ship ship)
        {
            return Vector3.Distance(ship.Position, GetExitPoint(in ship)) <= SimulationConsts.MovementPositionEpsilon;
        }

        private static Vector3 GetExitPoint(in Ship ship)
        {
            return ship.Warp.WarpDestination - ship.Warp.LockedDirection * SimulationConsts.WarpExitRadiusMetric;
        }

        private static void MoveAlongDirection(ref Ship ship, Vector3 direction, float speed, float deltaTime)
        {
            if (direction.sqrMagnitude <= 0f || speed <= 0f || deltaTime <= 0f)
                return; // Для движения нужна валидная скорость, длительность и зафиксированный курс.

            direction.Normalize();
            ship.Position += direction * speed * deltaTime;
            ship.Rotation = Quaternion.LookRotation(Vector3.forward, direction);
        }
    }
}
