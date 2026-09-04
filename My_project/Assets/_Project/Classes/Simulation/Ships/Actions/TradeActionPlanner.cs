using _Project.Scripts.Core.GameState;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using _Project.Scripts.Ships.Orders;
using _Project.Scripts.Simulation.Local;
using _Project.Trade;

namespace _Project.Scripts.Simulation.Ships
{
    /// <summary>Диспетчер торговых top-order в локальном и глобальном проходе.</summary>
    internal static class TradeActionPlanner
    {
        private struct TradeGalaxyCandidateCache
        {
            public bool IsResolved;
            public bool HasCandidate;
            public GalacticTradeCandidate Candidate;
        }

        public static void EnsureTradeActions(in LocalSimulationContext context)
        {
            if (!context.HasActiveSystem)
                return; // Локальный AI работает только в активной системе.

            var system = context.ActiveSystem.Value;
            var runtime = system.State;
            if (runtime == null)
                return;

            var galaxyCache = new TradeGalaxyCandidateCache();
            var ships = runtime.Ships;
            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                ApplyTradeOrder(ref ship, context.GameState, context.GameState.ActiveLocalSystemIndex, in system, ref galaxyCache); // Маршрутизация по типу top-order.
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

            var galaxyCache = new TradeGalaxyCandidateCache();
            var ships = runtime.Ships;
            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                ApplyTradeOrder(ref ship, gameState, currentSystemIndex, in system, ref galaxyCache); // Глобальная ветка для неактивных систем.
                ships[i] = ship;
            }
        }

        private static void ApplyTradeOrder(
            ref Ship ship,
            GameStateService gameState,
            int currentSystemIndex,
            in StarSys system,
            ref TradeGalaxyCandidateCache galaxyCache)
        {
            if (ship.TopOrder.Type == ETopShipOrderType.TradeInSystem)
            {
                TradeInSystemPlanner.TryPlan(ref ship, in system); // Локальная покупка/продажа внутри StarSys.
                return;
            }

            if (ship.TopOrder.Type == ETopShipOrderType.TradeGalaxy)
            {
                if (ship.TaskState.HasTasks)
                {
                    TradeDockActionAssigner.TryAssignFromTopMoveTask(ref ship); // При подходе к станции ставим Dock, даже если стек уже собран.
                    return; // Для занятых кораблей кэш кандидата не нужен.
                }

                if (!galaxyCache.IsResolved)
                {
                    galaxyCache.HasCandidate = TradeGalaxyPlanner.TryGetBestCandidate(gameState, currentSystemIndex, out galaxyCache.Candidate);
                    galaxyCache.IsResolved = true;
                }

                if (galaxyCache.HasCandidate)
                    TradeGalaxyPlanner.TryPlanWithCandidate(ref ship, gameState, currentSystemIndex, in galaxyCache.Candidate); // Межсистемный маршрут с jump-задачами.

                TradeDockActionAssigner.TryAssignFromTopMoveTask(ref ship); // После планирования сразу подхватываем первый док-этап.
            }
        }
    }
}
