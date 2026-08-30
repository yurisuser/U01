namespace _Project.Scripts.Stations
{
    /// <summary>Текущее состояние индустриального модуля станции.</summary>
    public sealed class IndustryModuleState : IStationModuleState
    {
        public int ResourceId; // ресурс, закреплённый за станцией
        public int DepositId; // депозит, закреплённый за станцией
        public int SourcePlanetIndex = -1; // индекс планеты в системе
        public int SourceMoonIndex = -1; // индекс луны на планете
        public int LastProductionTurn; // последний ход, учтённый производством
        public int ProductionProgressTurns; // накопленный прогресс текущего цикла в ходах
    }
}
