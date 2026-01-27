using System.Collections.Generic;
using _Project.Scripts.Core;

namespace _Project.Scripts.Stations
{
    /// <summary>Состояние док-модуля.</summary>
    public sealed class DockModuleState : IStationModuleState
    {
        public readonly List<UID> Occupied = new(); // текущие стыкованные корабли
        public readonly Queue<UID> Waiting = new(); // очередь на стыковку
    }
}
