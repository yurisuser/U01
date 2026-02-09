using _Project.Scripts.Core.GameState;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using _Project.Scripts.Ships.Orders;
using _Project.Scripts.Simulation.Local;

namespace _Project.Scripts.Simulation.Ships
{
    /// <summary>Диспетчер торговых top-order в локальном и глобальном проходе.</summary>
    internal static class TradeActionPlanner
    {
        public static void EnsureTradeActions(in LocalSimulationContext context)
        {
            if (!context.HasActiveSystem)
                return; // Локальный AI работает только в активной системе.

            var system = context.ActiveSystem.Value;
            var runtime = system.State;
            if (runtime == null)
                return;

            var ships = runtime.Ships;
            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                ApplyTradeOrder(ref ship, context.GameState, context.GameState.SelectedSystemIndex, in system); // Маршрутизация по типу top-order.
                ships[i] = ship;
            }
        }

        public static void EnsureTradeActionsForSystem(GameStateService gameState, int currentSystemIndex, ref StarSys system)
        {
            if (gameState == null)
                return;

            var runtime = system.State;
            if (runtime == null)
                return;

            var ships = runtime.Ships;
            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                ApplyTradeOrder(ref ship, gameState, currentSystemIndex, in system); // Глобальная ветка для неактивных систем.
                ships[i] = ship;
            }
        }

        private static void ApplyTradeOrder(ref Ship ship, GameStateService gameState, int currentSystemIndex, in StarSys system)
        {
            if (ship.TopOrder.Type == ETopShipOrderType.TradeInSystem)
            {
                TradeInSystemPlanner.TryPlan(ref ship, in system); // Локальная покупка/продажа внутри StarSys.
                return;
            }

            if (ship.TopOrder.Type == ETopShipOrderType.TradeGalaxy)
                TradeGalaxyPlanner.TryPlan(ref ship, gameState, currentSystemIndex); // Межсистемный маршрут с jump-задачами.
        }
    }
}
