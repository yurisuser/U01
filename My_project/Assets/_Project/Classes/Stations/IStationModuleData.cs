namespace _Project.Scripts.Stations
{
    /// <summary>Конфиг модуля станции (фиксированные параметры).</summary>
    public interface IStationModuleData
    {
        EStationModuleType ModuleType { get; } // тип модуля, для быстрых проверок/свитчей
    }
}
