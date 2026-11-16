namespace _Project.Scripts.Simulation.Ships.Movement
{
    /// <summary>Исчерпание бюджета прогноза отделено от невозможности выполнить маршрут.</summary>
    public enum EShipTravelEstimateStatus
    {
        InvalidInput = 0,
        Completed,
        Unavailable,
        StepLimitReached,
        LowerBound, // Доказанная нижняя граница времени, а не полный прогноз прибытия.
    }
}
