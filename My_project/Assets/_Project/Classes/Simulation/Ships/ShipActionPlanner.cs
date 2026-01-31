using _Project.Scripts.Simulation.Local;

namespace _Project.Scripts.Simulation.Ships
{
    public static class ShipActionPlanner
    {
        public static void EnsureTradeActions(in LocalSimulationContext context)
        {
            TradeActionPlanner.EnsureTradeActions(context);
        }

        public static void EnsureDockActions(in LocalSimulationContext context)
        {
            DockActionPlanner.EnsureDockActions(context);
        }
    }
}
