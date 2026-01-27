using System.Collections.Generic;

namespace _Project.Scripts.Stations
{
    /// <summary>Состояние карго-модуля.</summary>
    public sealed class CargoModuleState : IStationModuleState
    {
        public int Used; // занятое место
        public readonly Dictionary<int, int> Stock = new(); // ItemId → количество
    }
}
