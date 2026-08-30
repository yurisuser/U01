using System;
using System.Collections.Generic;
using _Project.DataAccess;
using _Project.CONST;
using _Project.Items;

namespace _Project.Scripts.Stations
{
    /// <summary>ТЕСТ: создаёт случайный склад и ордера станции для отладки торгового ИИ.</summary>
    public static class TestStationTradeBootstrap
    {
        private const int OrdersPerSide = 25;
        private const float MeanFill = 0.5f;
        private const float FillSpread = 0.3f;

        public static void InitializeRandomMarket(ref Station station, Random rng)
        {
            if (rng == null)
                rng = new Random();

            var cargoModule = EnsureModule(ref station, EStationModuleType.Storage);
            var tradeModule = EnsureModule(ref station, EStationModuleType.Trade);

            if (cargoModule?.State is not StorageModuleState cargoState)
                return;
            if (tradeModule?.State is not TradeModuleState tradeState)
                return;

            var items = CATALOG.Items;
            if (items == null || items.Count == 0)
                return;

            var ids = new List<int>(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.Id > 0)
                    ids.Add(item.Id);
            }

            if (ids.Count == 0)
                return;

            Shuffle(ids, rng);

            int total = ids.Count;
            int buyCount = Math.Min(OrdersPerSide, total / 2);
            int sellCount = Math.Min(OrdersPerSide, total - buyCount);

            for (int i = 0; i < buyCount; i++)
                AddBuyOrder(ids[i], cargoState, tradeState, rng);

            for (int i = 0; i < sellCount; i++)
                AddSellOrder(ids[buyCount + i], cargoState, tradeState, rng);
        }

        private static void AddBuyOrder(int itemId, StorageModuleState cargoState, TradeModuleState tradeState, Random rng)
        {
            var key = new ItemKey(ItemType.Item, itemId);
            int limit = TradeLimits.GetMaxAmount(key);
            if (limit <= 0)
                return;

            float fill = RandomRange(rng, MeanFill - FillSpread, MeanFill);
            int stock = (int)MathF.Round(limit * fill);
            cargoState.Cargo.SetAmount(key, stock);

            int target = (int)MathF.Round(limit * MeanFill);
            int amount = Math.Max(1, target - stock);
            int price = CalcPrice(itemId, fill);

            tradeState.OrdersBuy[key] = new OrderBy
            {
                Key = key,
                Price = price,
                Amount = amount
            };
        }

        private static void AddSellOrder(int itemId, StorageModuleState cargoState, TradeModuleState tradeState, Random rng)
        {
            var key = new ItemKey(ItemType.Item, itemId);
            int limit = TradeLimits.GetMaxAmount(key);
            if (limit <= 0)
                return;

            float fill = RandomRange(rng, MeanFill, MeanFill + FillSpread);
            int stock = (int)MathF.Round(limit * fill);
            cargoState.Cargo.SetAmount(key, stock);

            int target = (int)MathF.Round(limit * MeanFill);
            int amount = Math.Max(1, stock - target);
            int price = CalcPrice(itemId, fill);

            tradeState.OrdersSell[key] = new OrderSell
            {
                Key = key,
                Price = price,
                Amount = amount
            };
        }

        private static int CalcPrice(int itemId, float fill)
        {
            if (CATALOG.ItemsById == null || !CATALOG.ItemsById.TryGetValue(itemId, out var item))
                return 0;

            int basePrice = (int)item.Price;
            if (basePrice <= 0)
                return 0;

            float delta = (MeanFill - fill) / FillSpread * EconomyConstants.MaxPriceDelta;
            delta = Math.Clamp(delta, -EconomyConstants.MaxPriceDelta, EconomyConstants.MaxPriceDelta);
            float price = basePrice * (1f + delta);
            return Math.Max(1, (int)MathF.Round(price));
        }

        private static float RandomRange(Random rng, float min, float max)
        {
            if (min > max)
            {
                var tmp = min;
                min = max;
                max = tmp;
            }

            float t = (float)rng.NextDouble();
            return min + (max - min) * t;
        }

        private static void Shuffle(List<int> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private static StationModule EnsureModule(ref Station station, EStationModuleType type)
        {
            var modules = station.Modules;
            if (modules != null)
            {
                for (int i = 0; i < modules.Length; i++)
                {
                    if (modules[i].Type != type)
                        continue;

                    EnsureModuleState(modules[i]);
                    return modules[i];
                }
            }

            var newModule = new StationModule
            {
                Type = type,
                Level = 1,
                Data = CreateModuleData(type),
                State = CreateModuleState(type)
            };

            if (modules == null)
            {
                station.Modules = new[] { newModule };
            }
            else
            {
                var expanded = new StationModule[modules.Length + 1];
                Array.Copy(modules, expanded, modules.Length);
                expanded[^1] = newModule;
                station.Modules = expanded;
            }

            return newModule;
        }

        private static void EnsureModuleState(StationModule module)
        {
            if (module.State != null && module.Data != null)
                return;

            if (module.Data == null)
                module.Data = CreateModuleData(module.Type);
            if (module.State == null)
                module.State = CreateModuleState(module.Type);
        }

        private static IStationModuleData CreateModuleData(EStationModuleType type)
        {
            return type switch
            {
                EStationModuleType.Storage => new StorageModuleData(),
                EStationModuleType.Dock => new DockModuleData(),
                EStationModuleType.Trade => new TradeModuleData(),
                _ => null
            };
        }

        private static IStationModuleState CreateModuleState(EStationModuleType type)
        {
            return type switch
            {
                EStationModuleType.Storage => new StorageModuleState(),
                EStationModuleType.Dock => new DockModuleState(),
                EStationModuleType.Trade => new TradeModuleState(),
                _ => null
            };
        }
    }
}
