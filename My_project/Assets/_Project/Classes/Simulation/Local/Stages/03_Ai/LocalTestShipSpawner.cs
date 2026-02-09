namespace _Project.Scripts.Simulation.Local.Stages.Ai
{
    /// <summary>Временный спавнер кораблей для локального теста.</summary>
    public static class LocalTestShipSpawner
    {
        public static void RunPatrolPrototype(in LocalSimulationContext context, int shipTarget, float patrolRadius)
        {
            if (!LocalSpawnRuntimeResolver.TryResolve(in context, out var runtime, out var stations, out var systemIndex))
                return; // Контекст невалиден для спавна.

            LocalSpawnPopulation.EnsureShipCount(runtime, stations, shipTarget, patrolRadius);                // Доводим население до цели.
            LocalSpawnTopOrderAssigner.EnsureTradeGalaxyOrders(runtime, systemIndex, patrolRadius);           // Пустым кораблям выдаем межсистемный trade-order.
        }
    }
}
