using System.Collections.Generic;

namespace _Project.Scripts.Stations
{
    /// <summary>Состояние модуля хранилища.</summary>
    public sealed class StorageModuleState : IStationModuleState
    {
        public readonly Dictionary<int, int> Stock = new(); // ItemId → количество
    }
}
