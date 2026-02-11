using System;
using System.Collections.Generic;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Stations;
using _Project.Scripts.Trade.Models;

namespace _Project.Scripts.Trade.Services
{
    /// <summary>Поиск межсистемных торговых кандидатов с ограничением по хопам и оценкой yield.</summary>
    public static class GalacticTradeFinder
    {
        private const int DefaultMaxResults = 10; // Дефолтный размер выборки лучших кандидатов.
        private const int DefaultMaxHops = 6; // Дефолтный лимит хопов для маршрутов поиска.

        // --- Пайплайн: хардкор в начале файла ---
        private sealed class PipelineContext // Общий контекст прохода через стадии поиска.
        {
            public StarSys[] Galaxy;
            public HyperlinkEdge[] Edges;
            public int SystemsCount;
            public int CurrentSystemIndex;
            public int MaxResults;
            public int MaxHops;
            public float AvgYield;
            public bool OneHopMode;
            public List<int>[] Adjacency;

            public readonly List<SystemPair> SystemPairs = new List<SystemPair>(64); // Кандидатные пары систем.
            public readonly List<StationPair> StationPairs = new List<StationPair>(128); // Кандидатные пары станций.
            public readonly List<OrderMatch> OrderMatches = new List<OrderMatch>(256); // Совпадения ордеров sell/buy.
            public readonly List<GalacticTradeCandidate> Candidates = new List<GalacticTradeCandidate>(256); // Итоговые кандидаты.
        }

        private readonly struct SystemPair // Узел "система-продавец -> система-покупатель".
        {
            public readonly int SellerSystem;
            public readonly int BuyerSystem;
            public readonly int HopsToSeller;
            public readonly int HopsSellerToBuyer;

            public SystemPair(int sellerSystem, int buyerSystem, int hopsToSeller, int hopsSellerToBuyer)
            {
                SellerSystem = sellerSystem;
                BuyerSystem = buyerSystem;
                HopsToSeller = hopsToSeller;
                HopsSellerToBuyer = hopsSellerToBuyer;
            }
        }

        private readonly struct StationPair // Узел "станция-продавец -> станция-покупатель".
        {
            public readonly int SellerSystem;
            public readonly int BuyerSystem;
            public readonly int HopsToSeller;
            public readonly int HopsSellerToBuyer;
            public readonly Station SellerStation;
            public readonly Station BuyerStation;
            public readonly TradeModuleState SellerTrade;
            public readonly TradeModuleState BuyerTrade;

            public StationPair(
                int sellerSystem,
                int buyerSystem,
                int hopsToSeller,
                int hopsSellerToBuyer,
                in Station sellerStation,
                in Station buyerStation,
                TradeModuleState sellerTrade,
                TradeModuleState buyerTrade)
            {
                SellerSystem = sellerSystem;
                BuyerSystem = buyerSystem;
                HopsToSeller = hopsToSeller;
                HopsSellerToBuyer = hopsSellerToBuyer;
                SellerStation = sellerStation;
                BuyerStation = buyerStation;
                SellerTrade = sellerTrade;
                BuyerTrade = buyerTrade;
            }
        }

        private readonly struct OrderMatch // Узел "совпавший товар между sell/buy".
        {
            public readonly StationPair StationPair;
            public readonly int ItemId;
            public readonly int Amount;
            public readonly int SellPrice;
            public readonly int BuyPrice;

            public OrderMatch(in StationPair stationPair, int itemId, int amount, int sellPrice, int buyPrice)
            {
                StationPair = stationPair;
                ItemId = itemId;
                Amount = amount;
                SellPrice = sellPrice;
                BuyPrice = buyPrice;
            }
        }

        /// <summary>
        /// Ищет кандидатов, сортирует по yield/turn (относительно avgYield, если задан).
        /// maxHops: если &gt;=0, отбрасываем маршруты длиннее.
        /// maxResults: если &gt;0, возвращаем топ-N.
        /// </summary>
        public static List<GalacticTradeCandidate> FindCandidates(
            StarSys[] galaxy,
            HyperlinkEdge[] edges,
            float avgYield,
            int currentSystemIndex,
            int maxResults = DefaultMaxResults,
            int maxHops = DefaultMaxHops)
        {
            if (!TryCreateContext(galaxy, edges, avgYield, currentSystemIndex, maxResults, maxHops, out var context))
                return new List<GalacticTradeCandidate>(); // Вход невалиден или в 1-hop нет доступных систем.

            BuildSystemPairs(context);     // Стадия 1: готовим пары систем.
            BuildStationPairs(context);    // Стадия 2: раскрываем пары станций с Trade-модулями.
            BuildOrderMatches(context);    // Стадия 3: матчим sell/buy ордера по itemId.
            BuildCandidates(context);      // Стадия 4: собираем итоговые GalacticTradeCandidate.
            ScoreSortAndTrim(context.Candidates, context.AvgYield, context.MaxResults); // Стадия 5: score + top-N.
            return context.Candidates;
        }

        /// <summary>
        /// Ищет один лучший кандидат без промежуточного списка кандидатов.
        /// Нужен для горячего пути AI, где кораблю требуется только одна цель.
        /// </summary>
        public static bool TryFindBestCandidate(
            StarSys[] galaxy,
            HyperlinkEdge[] edges,
            float avgYield,
            int currentSystemIndex,
            out GalacticTradeCandidate bestCandidate,
            int maxHops = DefaultMaxHops)
        {
            bestCandidate = default;
            if (galaxy == null || galaxy.Length == 0)
                return false; // Пустая галактика.

            int systemsCount = galaxy.Length;
            int normalizedCurrent = currentSystemIndex;
            if (normalizedCurrent < 0 || normalizedCurrent >= systemsCount)
                normalizedCurrent = 0; // Фоллбек на 0-ю систему, если индекс вне диапазона.

            bool oneHopMode = maxHops == 1;
            List<int>[] adjacency = oneHopMode ? BuildAdjacency(edges, systemsCount) : null;
            if (oneHopMode && (adjacency == null || adjacency[normalizedCurrent].Count == 0))
                return false; // В радиусе 1 перехода нет систем-кандидатов.

            bool hasBest = false;
            float bestScore = float.MinValue;

            for (int sellerSys = 0; sellerSys < systemsCount; sellerSys++) // Перебираем системы-продавцы.
            {
                if (oneHopMode)
                {
                    if (sellerSys == normalizedCurrent)
                        continue; // В 1-hop режиме продавец — только сосед current.
                    if (!ContainsNeighbor(adjacency[normalizedCurrent], sellerSys))
                        continue; // Отсеиваем несоседние системы.
                }

                var sellerStations = galaxy[sellerSys].Stations;
                if (sellerStations == null || sellerStations.Length == 0)
                    continue; // В системе продавца нет станций.

                for (int buyerSys = 0; buyerSys < systemsCount; buyerSys++) // Перебираем системы-покупатели.
                {
                    if (buyerSys == sellerSys)
                        continue; // Продавец и покупатель не могут быть в одной системе.

                    int hopsToSeller;
                    int hopsSellerToBuyer;
                    if (oneHopMode)
                    {
                        if (!ContainsNeighbor(adjacency[sellerSys], buyerSys))
                            continue; // Покупатель должен быть соседом seller.
                        hopsToSeller = 1;
                        hopsSellerToBuyer = 1;
                    }
                    else
                    {
                        hopsSellerToBuyer = GalacticRouteFinder.GetHops(sellerSys, buyerSys, edges, systemsCount);
                        if (hopsSellerToBuyer < 0)
                            continue; // Нет маршрута seller -> buyer.
                        if (maxHops >= 0 && hopsSellerToBuyer > maxHops)
                            continue; // Превышен лимит хопов seller -> buyer.

                        hopsToSeller = GalacticRouteFinder.GetHops(normalizedCurrent, sellerSys, edges, systemsCount);
                        if (hopsToSeller < 0)
                            continue; // Нет маршрута current -> seller.
                        if (maxHops >= 0 && hopsToSeller > maxHops)
                            continue; // Превышен лимит хопов current -> seller.
                    }

                    var buyerStations = galaxy[buyerSys].Stations;
                    if (buyerStations == null || buyerStations.Length == 0)
                        continue; // В системе покупателя нет станций.

                    for (int si = 0; si < sellerStations.Length; si++) // Перебираем станции-продавцы.
                    {
                        if (!TryGetTradeState(sellerStations[si].Modules, out var sellerTrade))
                            continue; // На станции нет Trade-модуля.
                        if (sellerTrade.OrdersSell.Count == 0)
                            continue; // На станции нет ордеров продажи.

                        for (int bi = 0; bi < buyerStations.Length; bi++) // Перебираем станции-покупатели.
                        {
                            if (!TryGetTradeState(buyerStations[bi].Modules, out var buyerTrade))
                                continue; // На станции нет Trade-модуля.
                            if (buyerTrade.OrdersBuy.Count == 0)
                                continue; // На станции нет ордеров покупки.

                            foreach (var sellPair in sellerTrade.OrdersSell) // Перебираем товары seller-станции.
                            {
                                int itemId = sellPair.Key;
                                var sell = sellPair.Value;
                                if (!buyerTrade.OrdersBuy.TryGetValue(itemId, out var buy))
                                    continue; // Покупатель не берет этот товар.

                                int profitPerUnit = buy.Price - sell.Price;
                                if (profitPerUnit <= 0)
                                    continue; // Неприбыльная сделка.

                                int amount = Math.Min(sell.Amount, buy.Amount);
                                if (amount <= 0)
                                    continue; // По факту объем сделки нулевой.

                                var candidate = new GalacticTradeCandidate(
                                    sellerStations[si].Uid,
                                    sellerSys,
                                    buyerStations[bi].Uid,
                                    buyerSys,
                                    itemId,
                                    amount,
                                    sell.Price,
                                    buy.Price,
                                    hopsToSeller,
                                    hopsSellerToBuyer);
                                float score = GalacticTradeScoringService.ComputeScore(candidate, avgYield); // Считаем score сразу в потоке.

                                if (!hasBest || score > bestScore)
                                {
                                    hasBest = true; // Нашли первый или лучший по score вариант.
                                    bestScore = score;
                                    bestCandidate = candidate;
                                }
                            }
                        }
                    }
                }
            }

            return hasBest;
        }

        private static bool TryCreateContext(
            StarSys[] galaxy,
            HyperlinkEdge[] edges,
            float avgYield,
            int currentSystemIndex,
            int maxResults,
            int maxHops,
            out PipelineContext context)
        {
            context = null;
            if (galaxy == null || galaxy.Length == 0)
                return false; // Пустая галактика.

            int systemsCount = galaxy.Length;
            int normalizedCurrent = currentSystemIndex;
            if (normalizedCurrent < 0 || normalizedCurrent >= systemsCount)
                normalizedCurrent = 0; // Фоллбек на 0-ю систему, если индекс вне диапазона.

            bool oneHopMode = maxHops == 1;
            List<int>[] adjacency = oneHopMode ? BuildAdjacency(edges, systemsCount) : null;
            if (oneHopMode && (adjacency == null || adjacency[normalizedCurrent].Count == 0))
                return false; // В радиусе 1 перехода нет систем-кандидатов.

            context = new PipelineContext
            {
                Galaxy = galaxy,
                Edges = edges,
                SystemsCount = systemsCount,
                CurrentSystemIndex = normalizedCurrent,
                MaxResults = maxResults,
                MaxHops = maxHops,
                AvgYield = avgYield,
                OneHopMode = oneHopMode,
                Adjacency = adjacency
            };
            return true;
        }

        private static void BuildSystemPairs(PipelineContext context) // Стадия 1: формируем пары систем, которые проходят route-фильтры.
        {
            for (int sellerSys = 0; sellerSys < context.SystemsCount; sellerSys++) // Перебираем системы-продавцы.
            {
                if (context.OneHopMode)
                {
                    if (sellerSys == context.CurrentSystemIndex)
                        continue; // В 1-hop режиме продавец — только сосед current.
                    if (!ContainsNeighbor(context.Adjacency[context.CurrentSystemIndex], sellerSys))
                        continue; // Отсеиваем несоседние системы.
                }

                var sellerStations = context.Galaxy[sellerSys].Stations;
                if (sellerStations == null || sellerStations.Length == 0)
                    continue; // В системе продавца нет станций.

                for (int buyerSys = 0; buyerSys < context.SystemsCount; buyerSys++) // Перебираем системы-покупатели.
                {
                    if (!TryResolveHops(context, sellerSys, buyerSys, out int hopsToSeller, out int hopsSellerToBuyer))
                        continue; // Пара систем недостижима или не проходит лимит maxHops.

                    var buyerStations = context.Galaxy[buyerSys].Stations;
                    if (buyerStations == null || buyerStations.Length == 0)
                        continue; // В системе покупателя нет станций.

                    context.SystemPairs.Add(new SystemPair(sellerSys, buyerSys, hopsToSeller, hopsSellerToBuyer));
                }
            }
        }

        private static bool TryResolveHops(
            PipelineContext context,
            int sellerSys,
            int buyerSys,
            out int hopsToSeller,
            out int hopsSellerToBuyer)
        {
            hopsToSeller = -1;
            hopsSellerToBuyer = -1;
            if (buyerSys == sellerSys)
                return false; // Продавец и покупатель не могут быть в одной системе.

            if (context.OneHopMode)
            {
                // Временный быстрый режим:
                // current -> seller = 1 hop (seller уже сосед current),
                // seller -> buyer = 1 hop (buyer сосед seller).
                if (!ContainsNeighbor(context.Adjacency[sellerSys], buyerSys))
                    return false; // Покупатель должен быть соседом seller.
                hopsToSeller = 1;
                hopsSellerToBuyer = 1;
                return true;
            }

            hopsSellerToBuyer = GalacticRouteFinder.GetHops(sellerSys, buyerSys, context.Edges, context.SystemsCount);
            if (hopsSellerToBuyer < 0)
                return false; // Нет маршрута seller -> buyer.
            if (context.MaxHops >= 0 && hopsSellerToBuyer > context.MaxHops)
                return false; // Превышен лимит хопов seller -> buyer.

            hopsToSeller = GalacticRouteFinder.GetHops(context.CurrentSystemIndex, sellerSys, context.Edges, context.SystemsCount);
            if (hopsToSeller < 0)
                return false; // Нет маршрута current -> seller.
            if (context.MaxHops >= 0 && hopsToSeller > context.MaxHops)
                return false; // Превышен лимит хопов current -> seller.

            return true;
        }

        private static void BuildStationPairs(PipelineContext context) // Стадия 2: раскрываем пары систем в пары станций с Trade-модулями.
        {
            for (int i = 0; i < context.SystemPairs.Count; i++) // Перебираем пары систем, прошедшие route-фильтры.
            {
                var systemPair = context.SystemPairs[i];
                var sellerStations = context.Galaxy[systemPair.SellerSystem].Stations;
                var buyerStations = context.Galaxy[systemPair.BuyerSystem].Stations;
                if (sellerStations == null || buyerStations == null)
                    continue; // Защита от пустых массивов станций.

                for (int si = 0; si < sellerStations.Length; si++) // Перебираем станции-продавцы.
                {
                    if (!TryGetTradeState(sellerStations[si].Modules, out var sellerTrade))
                        continue; // На станции нет Trade-модуля.
                    if (sellerTrade.OrdersSell.Count == 0)
                        continue; // На станции нет ордеров продажи.

                    for (int bi = 0; bi < buyerStations.Length; bi++) // Перебираем станции-покупатели.
                    {
                        if (!TryGetTradeState(buyerStations[bi].Modules, out var buyerTrade))
                            continue; // На станции нет Trade-модуля.
                        if (buyerTrade.OrdersBuy.Count == 0)
                            continue; // На станции нет ордеров покупки.

                        context.StationPairs.Add(new StationPair(
                            systemPair.SellerSystem,
                            systemPair.BuyerSystem,
                            systemPair.HopsToSeller,
                            systemPair.HopsSellerToBuyer,
                            sellerStations[si],
                            buyerStations[bi],
                            sellerTrade,
                            buyerTrade));
                    }
                }
            }
        }

        private static void BuildOrderMatches(PipelineContext context) // Стадия 3: матчим ордера станций по itemId.
        {
            for (int i = 0; i < context.StationPairs.Count; i++) // Перебираем пары станций с валидными trade-состояниями.
            {
                var stationPair = context.StationPairs[i];
                foreach (var sellPair in stationPair.SellerTrade.OrdersSell) // Перебираем товары seller-станции.
                {
                    int itemId = sellPair.Key;
                    var sell = sellPair.Value;
                    if (!stationPair.BuyerTrade.OrdersBuy.TryGetValue(itemId, out var buy))
                        continue; // Покупатель не берет этот товар.

                    int profitPerUnit = buy.Price - sell.Price;
                    if (profitPerUnit <= 0)
                        continue; // Неприбыльная сделка.

                    int amount = Math.Min(sell.Amount, buy.Amount);
                    if (amount <= 0)
                        continue; // По факту объем сделки нулевой.

                    context.OrderMatches.Add(new OrderMatch(stationPair, itemId, amount, sell.Price, buy.Price));
                }
            }
        }

        private static void BuildCandidates(PipelineContext context) // Стадия 4: конвертируем матчи ордеров в GalacticTradeCandidate.
        {
            for (int i = 0; i < context.OrderMatches.Count; i++) // Перебираем валидные матчи sell/buy.
            {
                var match = context.OrderMatches[i];
                context.Candidates.Add(new GalacticTradeCandidate(
                    match.StationPair.SellerStation.Uid,
                    match.StationPair.SellerSystem,
                    match.StationPair.BuyerStation.Uid,
                    match.StationPair.BuyerSystem,
                    match.ItemId,
                    match.Amount,
                    match.SellPrice,
                    match.BuyPrice,
                    match.StationPair.HopsToSeller,
                    match.StationPair.HopsSellerToBuyer));
            }
        }

        private static void ScoreSortAndTrim(List<GalacticTradeCandidate> result, float avgYield, int maxResults)
        {
            result.Sort((a, b) => // Сортируем по score/yield в убывающем порядке.
            {
                float scoreA = GalacticTradeScoringService.ComputeScore(a, avgYield);
                float scoreB = GalacticTradeScoringService.ComputeScore(b, avgYield);
                return scoreB.CompareTo(scoreA);
            });

            if (maxResults > 0 && result.Count > maxResults)
                result.RemoveRange(maxResults, result.Count - maxResults); // Оставляем только top-N.
        }

        private static List<int>[] BuildAdjacency(HyperlinkEdge[] edges, int systemsCount)
        {
            var adjacency = new List<int>[systemsCount];
            for (int i = 0; i < systemsCount; i++)
                adjacency[i] = new List<int>(4);

            if (edges == null)
                return adjacency;

            for (int i = 0; i < edges.Length; i++)
            {
                var edge = edges[i];
                if (edge.A < 0 || edge.A >= systemsCount || edge.B < 0 || edge.B >= systemsCount || edge.A == edge.B)
                    continue;

                adjacency[edge.A].Add(edge.B);
                adjacency[edge.B].Add(edge.A);
            }

            return adjacency;
        }

        private static bool ContainsNeighbor(List<int> neighbors, int systemIndex)
        {
            if (neighbors == null || neighbors.Count == 0)
                return false;

            for (int i = 0; i < neighbors.Count; i++)
            {
                if (neighbors[i] == systemIndex)
                    return true;
            }

            return false;
        }

        private static bool TryGetTradeState(StationModule[] modules, out TradeModuleState tradeState)
        {
            tradeState = null;
            if (modules == null)
                return false;

            for (int i = 0; i < modules.Length; i++)
            {
                var module = modules[i];
                if (module == null || module.Type != EStationModuleType.Trade)
                    continue;

                tradeState = module.State as TradeModuleState;
                return tradeState != null;
            }

            return false;
        }
    }
}
