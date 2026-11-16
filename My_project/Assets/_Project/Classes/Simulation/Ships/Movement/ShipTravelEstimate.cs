using _Project.Scripts.Const;

namespace _Project.Scripts.Simulation.Ships.Movement
{
    /// <summary>Неизменяемая оценка времени маршрута в секундах симуляции с разбивкой по участкам.</summary>
    public readonly struct ShipTravelEstimate
    {
        public readonly EShipTravelEstimateStatus Status;
        public readonly double PreparationSeconds;
        public readonly double ChargeSeconds;
        public readonly double WarpSeconds;
        public readonly double BrakeSeconds;
        public readonly double ApproachSeconds;

        public bool IsComplete => Status == EShipTravelEstimateStatus.Completed;
        public double TotalSeconds => IsComplete || Status == EShipTravelEstimateStatus.LowerBound
            ? PreparationSeconds + ChargeSeconds + WarpSeconds + BrakeSeconds + ApproachSeconds
            : Status == EShipTravelEstimateStatus.Unavailable ? double.PositiveInfinity : double.NaN;
        public double TotalTurns => TotalSeconds / SimulationConsts.GlobalStepSeconds;

        public ShipTravelEstimate(EShipTravelEstimateStatus status, double preparationSeconds = 0,
            double chargeSeconds = 0, double warpSeconds = 0, double brakeSeconds = 0, double approachSeconds = 0)
        {
            Status = status;
            PreparationSeconds = preparationSeconds;
            ChargeSeconds = chargeSeconds;
            WarpSeconds = warpSeconds;
            BrakeSeconds = brakeSeconds;
            ApproachSeconds = approachSeconds;
        }
    }
}
