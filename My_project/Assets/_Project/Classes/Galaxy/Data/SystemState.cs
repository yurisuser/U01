using System.Collections.Generic;
using _Project.Scripts.Ships;

namespace _Project.Scripts.Galaxy.Data
{
    /// <summary>Динамическое состояние звёздной системы: корабли и прочие живые сущности.</summary>
    public sealed class SystemState
    {
        /// <summary>Список кораблей в системе.</summary>
        public List<Ship> Ships { get; } = new List<Ship>(50);
    }
}
