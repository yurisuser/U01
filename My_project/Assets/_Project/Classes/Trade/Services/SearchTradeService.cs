using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Stations;
using _Project.Scripts.Trade.Models;

namespace _Project.Scripts.Trade.Services
{
    /// <summary>Поиск выгодной сделки внутри системы.</summary>
    public static class SearchTradeService
    {
        /// <summary>
        /// Ищет лучшую пару buy/sell по станциям системы.
        /// Возвращает false, если выгодных сделок нет.
        /// </summary>
        public static bool TryFindBestInSystem(
            StarSys system,
            out TradeRoute route)
        {
            route = default;
            if (system.Stations == null || system.Stations.Length == 0)
                return false;

            bool found = false;
            int bestProfit = 0;

            for (int i = 0; i < system.Stations.Length; i++)
            {
                var sellerStation = system.Stations[i];
                var sellerTrade = FindTradeState(sellerStation.Modules);
                if (sellerTrade == null || sellerTrade.OrdersSell.Count == 0)
                    continue;

                for (int j = 0; j < system.Stations.Length; j++)
                {
                    if (i == j)
                        continue;

                    var buyerStation = system.Stations[j];
                    var buyerTrade = FindTradeState(buyerStation.Modules);
                    if (buyerTrade == null || buyerTrade.OrdersBuy.Count == 0)
                        continue;

                    foreach (var sellPair in sellerTrade.OrdersSell)
                    {
                        var sell = sellPair.Value;
                        if (!buyerTrade.OrdersBuy.TryGetValue(sell.ItemId, out var buy))
                            continue;

                        int profitPerUnit = buy.Price - sell.Price;
                        if (profitPerUnit <= 0)
                            continue;

                        int amount = sell.Amount < buy.Amount ? sell.Amount : buy.Amount;
                        if (amount <= 0)
                            continue;

                        int profit = profitPerUnit * amount;
                        if (!found || profit > bestProfit)
                        {
                            bestProfit = profit;
                            route = new TradeRoute(
                                sellerStation.Uid,
                                buyerStation.Uid,
                                sell.ItemId,
                                amount,
                                sell.Price,
                                buy.Price);
                            found = true;
                        }
                    }
                }
            }

            return found;
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
