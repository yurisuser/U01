namespace _Project.Scripts.Stations
{
    /// <summary>Базовый модуль станции.</summary>
    public sealed class StationModule
    {
        public EStationModuleType Type;     // тип модуля (док, карго и т.п.)
        public int Level;                   // уровень апгрейда модуля
        public IStationModuleData Data;     // статический конфиг
        public IStationModuleState State;   // динамическое состояние
    }
}
