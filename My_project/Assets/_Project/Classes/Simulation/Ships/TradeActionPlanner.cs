using _Project.Scripts.Const;
using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using _Project.Scripts.Ships.Actions;
using _Project.Scripts.Ships.Orders;
using _Project.Scripts.Simulation.Local;
using _Project.Scripts.Stations;
using _Project.Scripts.Trade.Models;
using _Project.Scripts.Trade.Services;
using _Project.Scripts.Simulation.Continuum;

namespace _Project.Scripts.Simulation.Ships
{
    internal static class TradeActionPlanner
    {
        public static void EnsureTradeActions(in LocalSimulationContext context)
        {
            if (!context.HasActiveSystem)
                return;

            var system = context.ActiveSystem.Value;
            var runtime = system.State;
            if (runtime == null)
                return;

            var ships = runtime.Ships;
            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                if (ship.TopOrder.Type == ETopShipOrderType.TradeInSystem)
                {
                    HandleTradeInSystem(ref ship, system);
                }
                else if (ship.TopOrder.Type == ETopShipOrderType.TradeGalaxy)
                {
                    HandleTradeGalaxy(ref ship, context);
                }

                ships[i] = ship;
            }
        }

        private static void HandleTradeInSystem(ref Ship ship, in StarSys system)
        {
            if (!ship.TaskState.HasTasks)
            {
                if (SearchTradeService.TryFindBestInSystem(system, out TradeRoute route))
                {
                    if (TryGetStation(in system, route.SellerUid, out var seller) &&
                        TryGetStation(in system, route.BuyerUid, out var buyer))
                    {
                        int amount = route.Amount;
                        int capacity = ship.Cargo.Capacity;
                        if (capacity > 0)
                        {
                            int free = capacity - ship.Cargo.Used;
                            if (free <= 0)
                                return;
                            if (amount > free)
                                amount = free;
                        }

                        ship.TaskState.PushTask(ShipTask.TradeSell(route.BuyerUid, route.ItemId, amount));
                        ship.TaskState.PushTask(ShipTask.MoveTo(
                            buyer.Position,
                            SimulationConsts.DestinationPointTolerance,
                            keepSpeed: true,
                            targetUid: route.BuyerUid));
                        ship.TaskState.PushTask(ShipTask.TradeBuy(route.SellerUid, route.ItemId, amount));
                        ship.TaskState.PushTask(ShipTask.MoveTo(
                            seller.Position,
                            SimulationConsts.DestinationPointTolerance,
                            keepSpeed: true,
                            targetUid: route.SellerUid));
                    }
                }
            }

            if (ship.CurrentAction.IsEmpty &&
                ship.TaskState.TryPeek(out var task) &&
                task.Type == ShipTaskType.MoveToPoint &&
                task.Params.MoveToPointParams.TargetUid.Id != 0)
            {
                ship.CurrentAction = new ShipAction
                {
                    Type = EShipActionType.Dock,
                    TargetUid = task.Params.MoveToPointParams.TargetUid,
                };
            }
        }

        private static void HandleTradeGalaxy(ref Ship ship, in LocalSimulationContext context)
        {
            if (ship.TaskState.HasTasks)
                return;

            var gameState = context.GameState;
            if (gameState == null)
                return;

            var galaxy = gameState.Galaxy;
            var edges = gameState.HyperlinkEdges;
            int currentSystemIndex = gameState.SelectedSystemIndex;

            var candidates = GalacticTradeFinder.FindCandidates(galaxy, edges, avgYield: 0f, currentSystemIndex: currentSystemIndex, maxResults: 1);
            if (candidates == null || candidates.Count == 0)
                return;

            var candidate = candidates[0];

            if (!TryGetStation(in galaxy[candidate.SellerSystemIndex], candidate.SellerUid, out var seller))
                return;
            if (!TryGetStation(in galaxy[candidate.BuyerSystemIndex], candidate.BuyerUid, out var buyer))
                return;

            var continuum = ContinuumService.Instance;
            if (continuum == null)
                return;

            if (!continuum.TryGetZone(currentSystemIndex, candidate.SellerSystemIndex, out var zoneToSeller))
                return;

            if (!continuum.TryGetZone(candidate.SellerSystemIndex, candidate.BuyerSystemIndex, out var zoneSellerToBuyer))
                return;

            int amount = candidate.Amount;
            int capacity = ship.Cargo.Capacity;
            if (capacity > 0)
            {
                int free = capacity - ship.Cargo.Used;
                if (free <= 0)
                    return;
                if (amount > free)
                    amount = free;
            }

            ship.TaskState.PushTask(ShipTask.TradeSell(candidate.BuyerUid, candidate.ItemId, amount));
            ship.TaskState.PushTask(ShipTask.MoveTo(
                buyer.Position,
                SimulationConsts.DestinationPointTolerance,
                keepSpeed: true,
                targetUid: candidate.BuyerUid));

            ship.TaskState.PushTask(ShipTask.JumpToSystem(candidate.BuyerSystemIndex));
            ship.TaskState.PushTask(ShipTask.MoveTo(
                zoneSellerToBuyer.Center,
                ContinuumConsts.EntryZoneRadius,
                keepSpeed: true));

            ship.TaskState.PushTask(ShipTask.TradeBuy(candidate.SellerUid, candidate.ItemId, amount));
            ship.TaskState.PushTask(ShipTask.MoveTo(
                seller.Position,
                SimulationConsts.DestinationPointTolerance,
                keepSpeed: true,
                targetUid: candidate.SellerUid));

            ship.TaskState.PushTask(ShipTask.JumpToSystem(candidate.SellerSystemIndex));
            ship.TaskState.PushTask(ShipTask.MoveTo(
                zoneToSeller.Center,
                ContinuumConsts.EntryZoneRadius,
                keepSpeed: true));
        }

        private static bool TryGetStation(in StarSys system, UID stationUid, out Station station)
        {
            var stations = system.Stations;
            if (stations != null)
            {
                for (int i = 0; i < stations.Length; i++)
                {
                    if (stations[i].Uid.Id == stationUid.Id)
                    {
                        station = stations[i];
                        return true;
                    }
                }
            }

            station = default;
            return false;
        }
    }
}
