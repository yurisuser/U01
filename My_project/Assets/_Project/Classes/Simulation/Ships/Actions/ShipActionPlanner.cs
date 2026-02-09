using _Project.Scripts.Simulation.Local;

namespace _Project.Scripts.Simulation.Ships
{
    /// <summary>Единая точка входа для AI-планнеров действий корабля.</summary>
    public static class ShipActionPlanner
    {
        public static void EnsureTradeActions(in LocalSimulationContext context)
        {
            TradeActionPlanner.EnsureTradeActions(context); // Торговые верхнеуровневые приказы.
        }

        public static void EnsureDockActions(in LocalSimulationContext context)
        {
            DockActionPlanner.EnsureDockActions(context); // Базовое поведение стыковки.
        }
    }
}
