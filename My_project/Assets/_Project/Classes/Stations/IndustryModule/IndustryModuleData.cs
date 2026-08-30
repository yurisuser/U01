namespace _Project.Scripts.Stations
{
    /// <summary>Постоянная конфигурация индустриального модуля станции.</summary>
    public sealed class IndustryModuleData : IStationModuleData
    {
        public EStationModuleType ModuleType => EStationModuleType.Industry;
    }
}
