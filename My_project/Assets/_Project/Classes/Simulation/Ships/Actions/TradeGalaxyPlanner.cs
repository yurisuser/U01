using _Project.Scripts.Const;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.Continuum;
using _Project.Scripts.Trade.Services;

namespace _Project.Scripts.Simulation.Ships
{
    /// <summary>Планирование межсистемного trade-маршрута через hyperlink/continuum.</summary>
    internal static class TradeGalaxyPlanner
    {
        public static void TryPlan(ref Ship ship, GameStateService gameState, int currentSystemIndex)
        {
            if (gameState == null || ship.TaskState.HasTasks)
                return; // Нужны данные галактики и пустой стек.

            var galaxy = gameState.Galaxy;
            var edges = gameState.HyperlinkEdges;
            var candidates = GalacticTradeFinder.FindCandidates(
                galaxy,
                edges,
                avgYield: 0f,
                currentSystemIndex: currentSystemIndex,
                maxResults: 1); // Берем только лучший маршрут.

            if (candidates == null || candidates.Count == 0)
                return;

            var candidate = candidates[0];
            if (!TryResolveTradeAnchorData(
                    galaxy,
                    currentSystemIndex,
                    candidate.SellerSystemIndex,
                    candidate.BuyerSystemIndex,
                    candidate.SellerUid,
                    candidate.BuyerUid,
                    out var seller,
                    out var buyer,
                    out var zoneToSeller,
                    out var zoneSellerToBuyer))
            {
                return; // Не удалось собрать валидные опорные данные маршрута.
            }

            int amount = FitToCargo(candidate.Amount, ship.Cargo.Capacity, ship.Cargo.Used);
            if (amount <= 0)
                return; // Нет свободного трюма.

            // Порядок push обратный к исполнению: сначала финал, потом старт.
            ship.TaskState.PushTask(ShipTaskBuilder.TradeSell(candidate.BuyerUid, candidate.ItemId, amount));
            ship.TaskState.PushTask(ShipTaskBuilder.MoveTo(
                buyer.Position,
                SimulationConsts.DestinationPointTolerance,
                keepSpeed: true,
                targetUid: candidate.BuyerUid));

            ship.TaskState.PushTask(ShipTaskBuilder.JumpToSystem(candidate.BuyerSystemIndex));
            ship.TaskState.PushTask(ShipTaskBuilder.MoveTo(
                zoneSellerToBuyer.Center,
                ContinuumConsts.EntryZoneRadius,
                keepSpeed: true)); // Выход к зоне перед межсистемным jump.

            ship.TaskState.PushTask(ShipTaskBuilder.TradeBuy(candidate.SellerUid, candidate.ItemId, amount));
            ship.TaskState.PushTask(ShipTaskBuilder.MoveTo(
                seller.Position,
                SimulationConsts.DestinationPointTolerance,
                keepSpeed: true,
                targetUid: candidate.SellerUid));

            ship.TaskState.PushTask(ShipTaskBuilder.JumpToSystem(candidate.SellerSystemIndex));
            ship.TaskState.PushTask(ShipTaskBuilder.MoveTo(
                zoneToSeller.Center,
                ContinuumConsts.EntryZoneRadius,
                keepSpeed: true)); // Первый jump из текущей системы к продавцу.
        }

        private static bool TryResolveTradeAnchorData(
            StarSys[] galaxy,
            int currentSystemIndex,
            int sellerSystemIndex,
            int buyerSystemIndex,
            _Project.Scripts.Core.UID sellerUid,
            _Project.Scripts.Core.UID buyerUid,
            out _Project.Scripts.Stations.Station seller,
            out _Project.Scripts.Stations.Station buyer,
            out ContinuumZone zoneToSeller,
            out ContinuumZone zoneSellerToBuyer)
        {
            seller = default;
            buyer = default;
            zoneToSeller = default;
            zoneSellerToBuyer = default;

            if (galaxy == null)
                return false;
            if (sellerSystemIndex < 0 || sellerSystemIndex >= galaxy.Length)
                return false;
            if (buyerSystemIndex < 0 || buyerSystemIndex >= galaxy.Length)
                return false;

            if (!TradePlannerStationResolver.TryGetStation(in galaxy[sellerSystemIndex], sellerUid, out seller))
                return false; // Продавец уже недоступен/удален.
            if (!TradePlannerStationResolver.TryGetStation(in galaxy[buyerSystemIndex], buyerUid, out buyer))
                return false; // Покупатель уже недоступен/удален.

            var continuum = ContinuumService.Instance;
            if (continuum == null)
                return false;

            if (!continuum.TryGetZone(currentSystemIndex, sellerSystemIndex, out zoneToSeller))
                return false; // Нет зоны старта jump из текущей системы.
            if (!continuum.TryGetZone(sellerSystemIndex, buyerSystemIndex, out zoneSellerToBuyer))
                return false; // Нет зоны jump между seller и buyer системами.

            return true;
        }

        private static int FitToCargo(int requestedAmount, int capacity, int used)
        {
            int amount = requestedAmount;
            if (capacity > 0)
            {
                int free = capacity - used;
                if (free <= 0)
                    return 0; // Трюм уже занят.

                if (amount > free)
                    amount = free; // Корректируем объем под остаток трюма.
            }

            return amount;
        }
    }
}
