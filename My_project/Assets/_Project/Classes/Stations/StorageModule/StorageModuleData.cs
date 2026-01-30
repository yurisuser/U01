namespace _Project.Scripts.Stations
{
    /// <summary>Конфиг модуля хранилища.</summary>
    public sealed class StorageModuleData : IStationModuleData
    {
        public int Capacity; // вместимость хранилища
        public EStationModuleType ModuleType => EStationModuleType.Storage; // идентификатор типа
    }
}
