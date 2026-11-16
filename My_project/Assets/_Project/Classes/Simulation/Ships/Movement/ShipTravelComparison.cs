namespace _Project.Scripts.Simulation.Ships.Movement
{
    /// <summary>Сравнение двух оценок; неизвестное время не выдаётся за бесконечное.</summary>
    public readonly struct ShipTravelComparison
    {
        public readonly ShipTravelEstimate Metric;
        public readonly ShipTravelEstimate Warp;

        public ShipTravelComparison(ShipTravelEstimate metric, ShipTravelEstimate warp)
        {
            Metric = metric;
            Warp = warp;
        }

        public EShipTravelMode PreferredMode
        {
            get
            {
                if (Warp.IsComplete && Metric.Status == EShipTravelEstimateStatus.LowerBound
                    && Warp.TotalSeconds < Metric.TotalSeconds)
                    return EShipTravelMode.Warp; // Варп быстрее даже теоретически лучшего метрического пути.
                if (Metric.IsComplete && Warp.IsComplete)
                    return Warp.TotalSeconds < Metric.TotalSeconds ? EShipTravelMode.Warp : EShipTravelMode.Metric;
                if (Metric.IsComplete && Warp.Status == EShipTravelEstimateStatus.Unavailable)
                    return EShipTravelMode.Metric;
                if (Warp.IsComplete && Metric.Status == EShipTravelEstimateStatus.Unavailable)
                    return EShipTravelMode.Warp;
                return EShipTravelMode.None; // Неполный прогноз не позволяет объявить один маршрут быстрее другого.
            }
        }
    }
}
