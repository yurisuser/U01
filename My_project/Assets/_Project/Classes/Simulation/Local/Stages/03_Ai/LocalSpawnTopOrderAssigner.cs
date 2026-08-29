using _Project.Scripts.Ships;
using _Project.Scripts.Simulation;
using _Project.Scripts.Simulation.Ships;

namespace _Project.Scripts.Simulation.Local.Stages.Ai
{
    /// <summary>Временное распределение начальных приказов новых и простаивающих кораблей.</summary>
    internal static class LocalSpawnTopOrderAssigner
    {
        public static void EnsureInitialOrders(LocalSysRuntimeContext runtime, int systemIndex, float patrolRadius)
        {
            var ships = runtime.Ships;
            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                _Project.Scripts.Simulation.AI.ShipInitialOrderAssigner.EnsureOrder(ref ship, systemIndex, patrolRadius);
                ships[i] = ship; // value-type: записываем обратно в список.
            }
        }
    }
}
