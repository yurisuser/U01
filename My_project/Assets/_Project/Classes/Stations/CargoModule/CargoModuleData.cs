namespace _Project.Scripts.Stations
{
    /// <summary>Конфиг карго-модуля.</summary>
    public sealed class CargoModuleData : IStationModuleData
    {
        public int Capacity; // вместимость слота карго
        public EStationModuleType ModuleType => EStationModuleType.Cargo; // идентификатор типа
    }
}
