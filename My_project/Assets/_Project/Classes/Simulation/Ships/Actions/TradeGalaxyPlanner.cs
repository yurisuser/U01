using System.Collections.Generic;
using _Project.Scripts.Const;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.Continuum;
using _Project.Trade;

namespace _Project.Scripts.Simulation.Ships
{
    /// <summary>Планирование межсистемного trade-маршрута через hyperlink/continuum.</summary>
    internal static class TradeGalaxyPlanner
    {
        private readonly struct RouteStep
        {
            public readonly int ToSystemIndex;
            public readonly UnityEngine.Vector3 ZoneCenter;

            public RouteStep(int toSystemIndex, in UnityEngine.Vector3 zoneCenter)
            {
                ToSystemIndex = toSystemIndex;
                ZoneCenter = zoneCenter;
            }
        }

        public static void TryPlan(ref Ship ship, GameStateService gameState, int currentSystemIndex)
        {
            if (gameState == null || ship.TaskState.HasTasks)
                return; // Нужны данные галактики и пустой стек.

            if (!TryGetBestCandidate(gameState, currentSystemIndex, out var candidate))
                return;

            TryPlanWithCandidate(ref ship, gameState, currentSystemIndex, in candidate);
        }

        public static bool TryGetBestCandidate(GameStateService gameState, int currentSystemIndex, out GalacticTradeCandidate candidate)
        {
            candidate = default;
            if (gameState == null)
                return false;

            var galaxy = gameState.Galaxy;
            var edges = gameState.HyperlinkEdges;
            if (galaxy == null || galaxy.Length == 0 || edges == null)
                return false;

            if (!GalacticTradeFinder.TryFindBestCandidate(
                galaxy,
                edges,
                avgYield: 0f,
                currentSystemIndex: currentSystemIndex,
                out candidate,
                maxHops: 1)) // Временный режим: отбор маршрутов только через соседние системы (1 hop).
            {
                return false;
            }

            return true; // Найден один лучший кандидат.
        }

        public static void TryPlanWithCandidate(ref Ship ship, GameStateService gameState, int currentSystemIndex, in GalacticTradeCandidate candidate)
        {
            if (gameState == null || ship.TaskState.HasTasks)
                return;

            var galaxy = gameState.Galaxy;
            var edges = gameState.HyperlinkEdges;
            if (galaxy == null || galaxy.Length == 0 || edges == null)
                return;

            if (!TryResolveTradeAnchorData(
                    galaxy,
                    candidate.SellerSystemIndex,
                    candidate.BuyerSystemIndex,
                    candidate.SellerUid,
                    candidate.BuyerUid,
                    out var seller,
                    out var buyer))
            {
                return; // Не удалось собрать валидные опорные данные маршрута.
            }

            var continuum = ContinuumService.Instance;
            if (continuum == null)
                return;

            var pathToSeller = GalacticRouteFinder.GetPath(currentSystemIndex, candidate.SellerSystemIndex, edges, galaxy.Length);
            var pathSellerToBuyer = GalacticRouteFinder.GetPath(candidate.SellerSystemIndex, candidate.BuyerSystemIndex, edges, galaxy.Length);
            if (!TryBuildRouteSteps(pathToSeller, continuum, out var toSellerSteps))
                return;
            if (!TryBuildRouteSteps(pathSellerToBuyer, continuum, out var sellerToBuyerSteps))
                return;

            int amount = FitToCargo(candidate.Amount, ship.Cargo.Capacity, ship.Cargo.Used);
            if (amount <= 0)
                return; // Нет свободного трюма.

            // Порядок push обратный к исполнению: сначала финал, потом старт.
            ship.TaskState.PushTask(ShipTaskBuilder.TradeSell(candidate.BuyerUid, candidate.Key, amount));
            ship.TaskState.PushTask(ShipTaskBuilder.MoveToPosition(
                buyer.Position,
                SimulationConsts.DestinationPointTolerance,
                keepSpeed: true,
                targetUid: candidate.BuyerUid));

            PushRouteSteps(ref ship, sellerToBuyerSteps); // Продавец -> Покупатель (может быть мультихоп).

            ship.TaskState.PushTask(ShipTaskBuilder.TradeBuy(candidate.SellerUid, candidate.Key, amount));
            ship.TaskState.PushTask(ShipTaskBuilder.MoveToPosition(
                seller.Position,
                SimulationConsts.DestinationPointTolerance,
                keepSpeed: true,
                targetUid: candidate.SellerUid));

            PushRouteSteps(ref ship, toSellerSteps); // Текущая -> Продавец (может быть мультихоп).
        }

        private static bool TryResolveTradeAnchorData(
            StarSys[] galaxy,
            int sellerSystemIndex,
            int buyerSystemIndex,
            _Project.Scripts.Core.UID sellerUid,
            _Project.Scripts.Core.UID buyerUid,
            out _Project.Scripts.Stations.Station seller,
            out _Project.Scripts.Stations.Station buyer)
        {
            seller = default;
            buyer = default;

            if (sellerSystemIndex < 0 || sellerSystemIndex >= galaxy.Length)
                return false;
            if (buyerSystemIndex < 0 || buyerSystemIndex >= galaxy.Length)
                return false;

            if (!TradePlannerStationResolver.TryGetStation(in galaxy[sellerSystemIndex], sellerUid, out seller))
                return false; // Продавец уже недоступен/удален.
            if (!TradePlannerStationResolver.TryGetStation(in galaxy[buyerSystemIndex], buyerUid, out buyer))
                return false; // Покупатель уже недоступен/удален.

            return true;
        }

        private static bool TryBuildRouteSteps(List<int> path, ContinuumService continuum, out List<RouteStep> steps)
        {
            steps = new List<RouteStep>(4);
            if (path == null || path.Count == 0)
                return false;
            if (path.Count == 1)
                return true; // Уже в целевой системе.

            for (int i = 0; i < path.Count - 1; i++)
            {
                int from = path[i];
                int to = path[i + 1];
                if (!continuum.TryGetZone(from, to, out var zone))
                    return false;

                steps.Add(new RouteStep(to, zone.Center));
            }

            return true;
        }

        private static void PushRouteSteps(ref Ship ship, List<RouteStep> steps)
        {
            if (steps == null || steps.Count == 0)
                return;

            // Пушим в обратном порядке, чтобы первым выполнился самый ранний переход маршрута.
            for (int i = steps.Count - 1; i >= 0; i--)
            {
                var step = steps[i];
                ship.TaskState.PushTask(ShipTaskBuilder.JumpToSystem(step.ToSystemIndex));
                ship.TaskState.PushTask(ShipTaskBuilder.MoveToPosition(
                    step.ZoneCenter,
                    ContinuumConsts.EntryZoneRadius,
                    keepSpeed: true));
            }
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
