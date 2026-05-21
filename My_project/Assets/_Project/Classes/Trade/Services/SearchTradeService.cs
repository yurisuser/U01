using _Project.Items;
using _Project.Scripts.Core;
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
        public static bool TryFindBestInSystem( // поиск лучшей сделки в системе
            StarSys system,
            out TradeRoute route)
        {
            route = default;
            if (system.Stations == null || system.Stations.Length == 0)
                return false;

            bool found = false; // флаг, что нашли сделку
            int bestProfit = 0;

            for (int i = 0; i < system.Stations.Length; i++) // перебор продавцов
            {
                var sellerStation = system.Stations[i];
                var sellerTrade = FindTradeState(sellerStation.Modules);
                if (sellerTrade == null || sellerTrade.OrdersSell.Count == 0)
                    continue;

                for (int j = 0; j < system.Stations.Length; j++) // перебор покупателей
                {
                    if (i == j)
                        continue;

                    var buyerStation = system.Stations[j];
                    var buyerTrade = FindTradeState(buyerStation.Modules);
                    if (buyerTrade == null || buyerTrade.OrdersBuy.Count == 0)
                        continue;

                    foreach (var sellPair in sellerTrade.OrdersSell)
                    {
                        var key = sellPair.Key;
                        var sell = sellPair.Value;
                        if (!buyerTrade.OrdersBuy.TryGetValue(key, out var buy))
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
                                key,
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

        /// <summary>Ищет лучшую пару buy/sell по конкретному товару внутри системы.</summary>
        public static bool TryFindBestInSystemForItem( // поиск сделки по конкретному товару
            StarSys system,
            ItemKey key,
            out TradeRoute route)
        {
            route = default;
            if (key.IsEmpty)
                return false;
            if (system.Stations == null || system.Stations.Length == 0)
                return false;

            bool found = false; // флаг, что нашли сделку
            int bestProfit = 0;

            for (int i = 0; i < system.Stations.Length; i++) // перебор продавцов
            {
                var sellerStation = system.Stations[i];
                var sellerTrade = FindTradeState(sellerStation.Modules);
                if (sellerTrade == null || sellerTrade.OrdersSell.Count == 0)
                    continue;

                if (!sellerTrade.OrdersSell.TryGetValue(key, out var sell))
                    continue;

                for (int j = 0; j < system.Stations.Length; j++) // перебор покупателей
                {
                    if (i == j)
                        continue;

                    var buyerStation = system.Stations[j];
                    var buyerTrade = FindTradeState(buyerStation.Modules);
                    if (buyerTrade == null || buyerTrade.OrdersBuy.Count == 0)
                        continue;

                    if (!buyerTrade.OrdersBuy.TryGetValue(key, out var buy))
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
                            key,
                            amount,
                            sell.Price,
                            buy.Price);
                        found = true;
                    }
                }
            }

            return found;
        }

        /// <summary>Ищет лучшую станцию-покупателя для товара.</summary>
        public static bool TryFindBestBuyerInSystem( // поиск лучшего покупателя
            StarSys system,
            ItemKey key,
            out UID buyerUid)
        {
            buyerUid = default;
            if (key.IsEmpty)
                return false;
            if (system.Stations == null || system.Stations.Length == 0)
                return false;

            bool found = false; // флаг, что нашли покупателя
            int bestPrice = 0;

            for (int i = 0; i < system.Stations.Length; i++) // перебор станций
            {
                var station = system.Stations[i];
                var trade = FindTradeState(station.Modules);
                if (trade == null || trade.OrdersBuy.Count == 0)
                    continue;

                if (!trade.OrdersBuy.TryGetValue(key, out var order))
                    continue;

                if (!found || order.Price > bestPrice)
                {
                    bestPrice = order.Price;
                    buyerUid = station.Uid;
                    found = true;
                }
            }

            return found;
        }

        public static bool TryFindBestSellerInSystem( // поиск лучшего продавца
            StarSys system,
            ItemKey key,
            out UID sellerUid)
        {
            sellerUid = default;
            if (key.IsEmpty)
                return false;
            if (system.Stations == null || system.Stations.Length == 0)
                return false;

            bool found = false; // флаг, что нашли продавца
            int bestPrice = int.MaxValue;

            for (int i = 0; i < system.Stations.Length; i++) // перебор станций
            {
                var station = system.Stations[i];
                var trade = FindTradeState(station.Modules);
                if (trade == null || trade.OrdersSell.Count == 0)
                    continue;

                if (!trade.OrdersSell.TryGetValue(key, out var order))
                    continue;

                if (!found || order.Price < bestPrice)
                {
                    bestPrice = order.Price;
                    sellerUid = station.Uid;
                    found = true;
                }
            }

            return found;
        }

        private static TradeModuleState FindTradeState(StationModule[] modules) // извлекаем торговый модуль
        {
            if (modules == null)
                return null;

            for (int i = 0; i < modules.Length; i++) // перебор модулей
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
