using _Project.Items;
using _Project.Scripts.Ships;
using _Project.Scripts.Stations;
using _Project.Scripts.Trade.Models;

namespace _Project.Scripts.Trade.Services
{
    /// <summary>Торговые операции (перенос товара) без участия денег у NPC.</summary>
    public static class TradeService
    {
        /// <summary>Купить товар со станции в трюм корабля (пока без денег).</summary>
        public static TradeResult TryBuy(ref Ship ship, CargoModuleState stationCargo, ref OrderSell order, int amount)
        {
            if (stationCargo == null || amount <= 0)
                return TradeResult.Fail(ETradeFailReason.InvalidInput);

            if (!stationCargo.Stock.TryGetValue(order.ItemId, out var stock) || stock <= 0)
                return TradeResult.Fail(ETradeFailReason.NotEnoughStock);

            int freeSpace = GetFreeCargoSpace(in ship);
            if (freeSpace <= 0)
                return TradeResult.Fail(ETradeFailReason.NotEnoughCargoSpace);

            int moved = Min(amount, stock, freeSpace);
            if (moved <= 0)
                return TradeResult.Fail(ETradeFailReason.NotEnoughStock);

            stationCargo.Stock[order.ItemId] = stock - moved;
            AddToCargo(ref ship, order.Type, order.ItemId, moved);
            order.Amount = Max(0, order.Amount - moved);

            return TradeResult.Ok(moved);
        }

        /// <summary>Продать товар с корабля на станцию (пока без денег).</summary>
        public static TradeResult TrySell(ref Ship ship, CargoModuleState stationCargo, ref OrderBy order, int amount)
        {
            if (stationCargo == null || amount <= 0)
                return TradeResult.Fail(ETradeFailReason.InvalidInput);

            int available = GetCargoAmount(in ship, order.Type, order.ItemId);
            if (available <= 0)
                return TradeResult.Fail(ETradeFailReason.NotEnoughStock);

            int moved = Min(amount, available, order.Amount > 0 ? order.Amount : amount);
            if (moved <= 0)
                return TradeResult.Fail(ETradeFailReason.NotEnoughStock);

            stationCargo.Stock.TryGetValue(order.ItemId, out var stock);
            stationCargo.Stock[order.ItemId] = stock + moved;
            RemoveFromCargo(ref ship, order.Type, order.ItemId, moved);
            order.Amount = Max(0, order.Amount - moved);

            return TradeResult.Ok(moved);
        }

        private static int GetFreeCargoSpace(in Ship ship)
        {
            int used = 0;
            var list = ship.CargoList;
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                    used += list[i].Quantity;
            }

            return Max(0, ship.CargoCapacity - used);
        }

        private static int GetCargoAmount(in Ship ship, ItemType type, int itemId)
        {
            var list = ship.CargoList;
            if (list == null)
                return 0;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Type == type && list[i].Id == itemId)
                    return list[i].Quantity;
            }

            return 0;
        }

        private static void AddToCargo(ref Ship ship, ItemType type, int itemId, int amount)
        {
            if (ship.CargoList == null)
                ship.CargoList = new System.Collections.Generic.List<ItemStack>();

            var list = ship.CargoList;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Type == type && list[i].Id == itemId)
                {
                    list[i] = new ItemStack(type, itemId, list[i].Quantity + amount);
                    return;
                }
            }

            list.Add(new ItemStack(type, itemId, amount));
        }

        private static void RemoveFromCargo(ref Ship ship, ItemType type, int itemId, int amount)
        {
            var list = ship.CargoList;
            if (list == null)
                return;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Type != type || list[i].Id != itemId)
                    continue;

                int left = list[i].Quantity - amount;
                if (left <= 0)
                    list.RemoveAt(i);
                else
                    list[i] = new ItemStack(type, itemId, left);

                return;
            }
        }

        private static int Min(int a, int b, int c)
        {
            int m = a < b ? a : b;
            return m < c ? m : c;
        }

        private static int Max(int a, int b)
        {
            return a > b ? a : b;
        }
    }
}
