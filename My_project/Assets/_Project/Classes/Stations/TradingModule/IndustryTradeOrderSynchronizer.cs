using System;
using _Project.DataAccess;
using _Project.Scripts.Galaxy.Data;

namespace _Project.Scripts.Stations
{
    /// <summary>Синхронизирует ордера продажи индустрии с фактическими остатками станции.</summary>
    public static class IndustryTradeOrderSynchronizer
    {
        public static void Refresh(in StarSys system)
        {
            if (system.Stations == null)
                return;

            for (int i = 0; i < system.Stations.Length; i++)
                Refresh(in system.Stations[i]);
        }

        private static void Refresh(in Station station)
        {
            if (station.Modules == null || station.Cargo == null)
                return;
            if (!TryGetTradeModule(station.Modules, out var tradeData, out var tradeState))
                return;

            for (int i = 0; i < station.Modules.Length; i++)
            {
                var module = station.Modules[i];
                if (module == null ||
                    module.Type != EStationModuleType.Industry ||
                    module.Data is not IndustryModuleData industryData ||
                    industryData.Recipe?.Outputs == null)
                    continue;

                var outputs = industryData.Recipe.Outputs;
                for (int j = 0; j < outputs.Length; j++)
                {
                    var key = outputs[j].Key;
                    if (key.IsEmpty)
                        continue;

                    int stock = station.Cargo.GetAmount(key);
                    int price = ResolvePrice(key.Id, tradeData, tradeState, key);
                    if (price <= 0)
                        continue;

                    tradeState.OrdersSell[key] = new OrderSell
                    {
                        Key = key,
                        Price = price,
                        Amount = Math.Max(0, stock)
                    };
                }
            }
        }

        private static int ResolvePrice(
            int itemId,
            TradeModuleData tradeData,
            TradeModuleState tradeState,
            _Project.Items.ItemKey key)
        {
            if (tradeState.OrdersSell.TryGetValue(key, out var current) && current.Price > 0)
                return current.Price;
            if (CATALOG.ItemsById == null || !CATALOG.ItemsById.TryGetValue(itemId, out var item) || item.Price <= 0f)
                return 0;

            float multiplier = tradeData?.PriceSellMultiplier ?? 1f;
            return Math.Max(1, (int)MathF.Round(item.Price * multiplier));
        }

        private static bool TryGetTradeModule(
            StationModule[] modules,
            out TradeModuleData data,
            out TradeModuleState state)
        {
            for (int i = 0; i < modules.Length; i++)
            {
                var module = modules[i];
                if (module == null || module.Type != EStationModuleType.Trade)
                    continue;

                data = module.Data as TradeModuleData;
                state = module.State as TradeModuleState;
                return state != null;
            }

            data = null;
            state = null;
            return false;
        }
    }
}
