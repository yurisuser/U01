namespace _Project.Scripts.Stations
{
    /// <summary>Тикает модули станции.</summary>
    public static class StationTickService
    {
        public static void TickActive(ref Station station, float deltaTime)
        {
            // Тут будет логика активной системы: проверка доков/карго, прогресс задач.
        }

        public static void TickOffline(ref Station station)
        {
            // Тут батч за один внутриигровой ход для неактивной системы.
        }
    }
}
