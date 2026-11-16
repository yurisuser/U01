using UnityEngine;
using _Project.Scripts.Const;
using _Project.Scripts.Ships;

namespace _Project.Scripts.Simulation.Ships.Movement
{
    /// <summary>Прогнозирует два маршрута с начала хода на независимых копиях кинематики корабля.</summary>
    public static class ShipTravelPlanner
    {
        /// <summary>Шаг передаёт вызывающая симуляция; прогноз учитывает допуск и KeepSpeed задачи, но не меняет мир.</summary>
        public static ShipTravelComparison Estimate(in Ship ship, in MoveToPointParams move, float metricStepSeconds)
        {
            if (!IsValid(in ship, in move, metricStepSeconds))
                return new ShipTravelComparison(default, default); // Активный варп нельзя переоценивать как новый свободный маршрут.

            var warp = PredictWarp(in ship, in move, metricStepSeconds);
            double metricMaxSpeed = ShipSpeed.GetMetricMaxSpeed(in ship);
            double lowerBound = metricMaxSpeed > 0
                ? Mathf.Max(0f, Vector3.Distance(ship.Position, move.Destination) - move.Tolerance) / metricMaxSpeed
                : double.PositiveInfinity;
            if (warp.IsComplete && warp.TotalSeconds < lowerBound)
            {
                var bound = new ShipTravelEstimate(EShipTravelEstimateStatus.LowerBound, approachSeconds: lowerBound);
                return new ShipTravelComparison(bound, warp); // Повороты, разгон и торможение могут только увеличить метрическое время.
            }

            var metricShip = ship.Clone(); // Общие ссылки на ИИ и задачи не используются и не изменяются.
            int budget = SimulationConsts.TravelPredictionMaxSteps;
            var status = PredictMetricLeg(ref metricShip, in move, metricStepSeconds, ref budget, out double metricSeconds);
            var metric = new ShipTravelEstimate(status, approachSeconds: metricSeconds);
            return new ShipTravelComparison(metric, warp);
        }

        private static ShipTravelEstimate PredictWarp(in Ship source, in MoveToPointParams move, float stepSeconds)
        {
            if (ShipMetricMotion.HasArrived(in source, in move) || !CanMove(in source) || ShipSpeed.GetWarpSpeed(in source) <= 0f)
                return new ShipTravelEstimate(EShipTravelEstimateStatus.Unavailable); // Нет необходимости или возможности начинать варп.

            var ship = source.Clone();
            ShipWarpProcessor.TryRequestWarp(ref ship, move.Destination);
            var preparation = move;
            preparation.KeepSpeed = true; // Подготовка требует максимальной скорости, а не остановки у цели.
            preparation.Tolerance = 0f;
            double prep = 0, charge = 0, warp = 0, brake = 0;
            bool started = false;
            int budget = SimulationConsts.TravelPredictionMaxSteps;
            int tickInTurn = 0;
            int ticksPerTurn = Mathf.CeilToInt(SimulationConsts.GlobalStepSeconds / stepSeconds);

            while (budget-- > 0) // Подготовку считаем по тикам, заблокированные фазы — полными ходами.
            {
                if (tickInTurn == 0)
                {
                    if (!started && ShipMetricMotion.HasArrived(in ship, in move))
                        return new ShipTravelEstimate(EShipTravelEstimateStatus.Unavailable, prep); // Прибытие во время подготовки важнее нового заряда.
                    ShipWarpProcessor.Process(ref ship, 0f, true); // Тот же процессор и тот же порядок границ, что при исполнении.
                    if (started && ship.Warp.Phase == EShipWarpPhase.Metric)
                    {
                        var status = PredictMetricLeg(ref ship, in move, stepSeconds, ref budget, out double approach);
                        return new ShipTravelEstimate(status, prep, charge, warp, brake, approach);
                    }

                    if (ship.Warp.Phase != EShipWarpPhase.Metric)
                    {
                        started = true;
                        var phase = ship.Warp.Phase;
                        ShipWarpProcessor.Process(ref ship, SimulationConsts.GlobalStepSeconds, false);
                        switch (phase)
                        {
                            case EShipWarpPhase.Charging: charge += SimulationConsts.GlobalStepSeconds; break;
                            case EShipWarpPhase.Warp: warp += SimulationConsts.GlobalStepSeconds; break;
                            case EShipWarpPhase.MetricBrake: brake += SimulationConsts.GlobalStepSeconds; break;
                        }
                        continue;
                    }

                    if (ShipWarpProcessor.IsAlignedAtMaxSpeed(in ship, move.Destination))
                        return new ShipTravelEstimate(EShipTravelEstimateStatus.Unavailable, prep); // Готовый корабль не принят процессором: для заряда не осталось пути.
                }

                if (ShipMetricMotion.HasArrived(in ship, in move))
                    return new ShipTravelEstimate(EShipTravelEstimateStatus.Unavailable, prep); // До готовности варпа уже достигнута зона назначения.

                float delta = GetTickDuration(stepSeconds, tickInTurn, ticksPerTurn);
                ShipMetricMotion.Step(ref ship, in preparation, delta);
                prep += delta;
                tickInTurn = (tickInTurn + 1) % ticksPerTurn;
            }

            return new ShipTravelEstimate(EShipTravelEstimateStatus.StepLimitReached, prep, charge, warp, brake);
        }

        private static EShipTravelEstimateStatus PredictMetricLeg(ref Ship ship, in MoveToPointParams move,
            float stepSeconds, ref int budget, out double seconds)
        {
            seconds = 0;
            if (ShipMetricMotion.HasArrived(in ship, in move))
                return EShipTravelEstimateStatus.Completed;
            if (!CanMove(in ship))
                return EShipTravelEstimateStatus.Unavailable; // Нулевой двигатель или невозможность разогнаться с места.

            int ticksPerTurn = Mathf.CeilToInt(SimulationConsts.GlobalStepSeconds / stepSeconds);
            int tickInTurn = 0;
            while (budget-- > 0)
            {
                float delta = GetTickDuration(stepSeconds, tickInTurn, ticksPerTurn);
                ShipMetricMotion.Step(ref ship, in move, delta);
                seconds += delta;
                if (ShipMetricMotion.HasArrived(in ship, in move))
                    return EShipTravelEstimateStatus.Completed; // Время включает последний тик прибытия, как у исполнителя.
                tickInTurn = (tickInTurn + 1) % ticksPerTurn;
            }
            return EShipTravelEstimateStatus.StepLimitReached;
        }

        private static float GetTickDuration(float stepSeconds, int tickInTurn, int ticksPerTurn)
        {
            return tickInTurn == ticksPerTurn - 1
                ? (float)(SimulationConsts.GlobalStepSeconds - (double)stepSeconds * tickInTurn)
                : stepSeconds; // Последний тик заканчиваем точно на границе хода, без накопления временной ошибки.
        }

        private static bool CanMove(in Ship ship)
        {
            return ShipSpeed.GetMetricMaxSpeed(in ship) > 0f
                && (ship.CurrentSpeed > 0f || ship.Stats.Acceleration > 0f || ship.Stats.Agility > 0f);
        }

        private static bool IsValid(in Ship ship, in MoveToPointParams move, float stepSeconds)
        {
            return ship != null && ship.Warp.Phase == EShipWarpPhase.Metric && !ship.Warp.HasWarpDestination
                && IsFinite(stepSeconds) && stepSeconds > 0f && stepSeconds <= SimulationConsts.GlobalStepSeconds
                && SimulationConsts.GlobalStepSeconds / stepSeconds <= SimulationConsts.TravelPredictionMaxSteps
                && IsFinite(move.Tolerance) && move.Tolerance >= 0f
                && IsFinite(move.Destination) && IsFinite(ship.Position)
                && Mathf.Abs(move.Destination.z - ship.Position.z) <= SimulationConsts.MovementPositionEpsilon
                && IsFinite(ship.CurrentSpeed) && ship.CurrentSpeed >= 0f
                && IsFinite(ship.Stats.MetricSpeed) && ship.Stats.MetricSpeed >= 0f
                && IsFinite(ship.Stats.WarpSpeed) && ship.Stats.WarpSpeed >= 0f
                && IsFinite(ship.Stats.Acceleration) && ship.Stats.Acceleration >= 0f
                && IsFinite(ship.Stats.Agility) && ship.Stats.Agility >= 0f
                && IsFinite(ship.Rotation.x) && IsFinite(ship.Rotation.y)
                && IsFinite(ship.Rotation.z) && IsFinite(ship.Rotation.w)
                && Quaternion.Dot(ship.Rotation, ship.Rotation) > 0f;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static bool IsFinite(in Vector3 value) => IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
    }
}
