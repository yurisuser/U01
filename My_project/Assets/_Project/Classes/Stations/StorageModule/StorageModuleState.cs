using _Project.Items;

namespace _Project.Scripts.Stations
{
    /// <summary>Состояние модуля хранилища.</summary>
    public sealed class StorageModuleState : IStationModuleState
    {
        public Cargo Cargo = new();
    }
}
