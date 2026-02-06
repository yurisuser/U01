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
            int maxResults = 10,
            int maxHops = 6)
        {
            var result = new List<GalacticTradeCandidate>();
            if (galaxy == null || galaxy.Length == 0)
                return result;

            int systemsCount = galaxy.Length;
            if (currentSystemIndex < 0 || currentSystemIndex >= systemsCount)
                currentSystemIndex = 0;
            for (int sellerSys = 0; sellerSys < systemsCount; sellerSys++)
            {
                var sellerStations = galaxy[sellerSys].Stations;
                if (sellerStations == null || sellerStations.Length == 0)
                    continue;

                for (int buyerSys = 0; buyerSys < systemsCount; buyerSys++)
                {
                    if (buyerSys == sellerSys)
                        continue;

                    int hopsSellerToBuyer = GalacticRouteFinder.GetHops(sellerSys, buyerSys, edges, systemsCount);
                    if (hopsSellerToBuyer < 0)
                        continue;
                    if (maxHops >= 0 && hopsSellerToBuyer > maxHops)
                        continue;

                    int hopsToSeller = GalacticRouteFinder.GetHops(currentSystemIndex, sellerSys, edges, systemsCount);
                    if (hopsToSeller < 0)
                        continue;
                    if (maxHops >= 0 && hopsToSeller > maxHops)
                        continue;

                    var buyerStations = galaxy[buyerSys].Stations;
                    if (buyerStations == null || buyerStations.Length == 0)
                        continue;

                    // Перебираем продавцов/покупателей
                    for (int si = 0; si < sellerStations.Length; si++)
                    {
                        var sellerTrade = FindTradeState(sellerStations[si].Modules);
                        if (sellerTrade == null || sellerTrade.OrdersSell.Count == 0)
                            continue;

                        foreach (var sellPair in sellerTrade.OrdersSell)
                        {
                            int itemId = sellPair.Key;
                            var sell = sellPair.Value;
                            for (int bi = 0; bi < buyerStations.Length; bi++)
                            {
                                var buyerTrade = FindTradeState(buyerStations[bi].Modules);
                                if (buyerTrade == null || buyerTrade.OrdersBuy.Count == 0)
                                    continue;
                                if (!buyerTrade.OrdersBuy.TryGetValue(itemId, out var buy))
                                    continue;

                                int profitPerUnit = buy.Price - sell.Price;
                                if (profitPerUnit <= 0)
                                    continue;

                                int amount = Math.Min(sell.Amount, buy.Amount);
                                if (amount <= 0)
                                    continue;

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

                                result.Add(candidate);
                            }
                        }
                    }
                }
            }

            // Сортируем по yield/score
            result.Sort((a, b) =>
            {
                float sa = GalacticTradeScoringService.ComputeScore(a, avgYield);
                float sb = GalacticTradeScoringService.ComputeScore(b, avgYield);
                return sb.CompareTo(sa);
            });

            if (maxResults > 0 && result.Count > maxResults)
                result.RemoveRange(maxResults, result.Count - maxResults);

            return result;
        }

        private static TradeModuleState FindTradeState(StationModule[] modules)
        {
            if (modules == null)
                return null;

            for (int i = 0; i < modules.Length; i++)
            {
                var module = modules[i];
                if (module == null || module.Type != EStationModuleType.Trade)
                    continue;

                return module.State as TradeModuleState;
            }

            return null;
        }
    }
}
