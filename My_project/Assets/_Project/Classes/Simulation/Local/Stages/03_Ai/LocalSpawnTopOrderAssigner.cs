using _Project.Scripts.Ships;
using _Project.Scripts.Simulation;
using _Project.Scripts.Simulation.Ships;

namespace _Project.Scripts.Simulation.Local.Stages.Ai
{
    /// <summary>Назначение базового top-order для freshly spawned/idle кораблей.</summary>
    internal static class LocalSpawnTopOrderAssigner
    {
        public static void EnsureTradeGalaxyOrders(LocalSysRuntimeContext runtime, int systemIndex, float patrolRadius)
        {
            var ships = runtime.Ships;
            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                _Project.Scripts.Simulation.Ships.TradeTopOrderAssigner.EnsureTradeGalaxyOrder(ref ship, systemIndex, patrolRadius); // Явно вызываем общий assigner, чтобы не зависеть от using-контекста.
                ships[i] = ship; // value-type: записываем обратно в список.
            }
        }
    }
}
