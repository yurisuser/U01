using _Project.DataAccess;
using _Project.Items;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using _Project.Scripts.Stations;
using _Project.Scripts.Trade.Models;

namespace _Project.Scripts.Simulation.Local.Stages.Interaction
{
    /// <summary>Исполнение action TradeSell на станции-покупателе.</summary>
    internal static class TradeSellInteraction
    {
        public static void Process( // обработка продажи
            ref Ship ship,
            ref StarSys system,
            in Station targetStation,
            TradeModuleState tradeState)
        {
            if (!tradeState.OrdersBuy.TryGetValue(ship.CurrentAction.ItemId, out var order)) // ордер покупки отсутствует
            {
                TradeInteractionLogger.LogTradeOrderMissing("Buy", ship.Uid.Id, targetStation.Uid.Id, ship.CurrentAction.ItemId);
                if (TryForcedSell(ref ship, ship.CurrentAction.ItemId, ship.CurrentAction.Amount, out var soldAmount, out var unitPrice))
                {
                    TradeInteractionLogger.LogTradeForcedSell(ship.Uid.Id, targetStation.Uid.Id, ship.CurrentAction.ItemId, soldAmount, unitPrice);
                    ship.TaskState.Pop(); // Закрываем текущую TradeSell-задачу принудительной продажей.
                    TradeInteractionHelpers.UndockSuccess(ref ship);
                }
                else
                {
                    TradeInteractionHelpers.FailAndResetTrade(ref ship); // Нечего продавать — сбрасываем сценарий.
                }

                return;
            }

            int amount = CalcSellAmount(in ship, in order);
            if (amount <= 0) // нечего продавать
            {
                TradeInteractionHelpers.FailAndResetTrade(ref ship); // Нет товара/объема для сделки.
                return;
            }

            var offer = new TradeOffer(
                ship,
                targetStation,
                _Project.Items.ItemType.Item,
                order.ItemId,
                amount,
                order.Price,
                system.Uid.Id);

            TradeInteractionLogger.LogTradeStart("Sell", ship.Uid.Id, targetStation.Uid.Id, order.ItemId, amount, order.Price);

            var result = _Project.Scripts.Trade.Services.TradeService.Execute(offer);
            if (!result.Success) // сделка не прошла
            {
                TradeInteractionLogger.LogTradeFailed("Sell", ship.Uid.Id, targetStation.Uid.Id, order.ItemId, amount, result.FailReason);
                TradeInteractionHelpers.FailAndResetTrade(ref ship); // Ошибку продажи не лечим частично.
                return;
            }

            ApplyOrderBuyDelta(ref tradeState, order.ItemId, amount); // Уменьшаем встречный buy-ордер станции.
            ship.TaskState.Pop();                                     // Sell-задача исполнена.
            TradeInteractionHelpers.UndockSuccess(ref ship);          // Возвращаем корабль в полет.
        }

        private static int CalcSellAmount(in Ship ship, in OrderBy order) // расчет объема продажи
        {
            int amount = ship.CurrentAction.Amount;
            if (order.Amount < amount) // ограничение ордера
                amount = order.Amount;

            int available = ship.Cargo.GetAmount(_Project.Items.ItemType.Item, order.ItemId);
            if (available < amount) // ограничение трюма
                amount = available; // Нельзя продать больше, чем есть в трюме.

            return amount;
        }

        private static void ApplyOrderBuyDelta(ref TradeModuleState tradeState, int itemId, int amount) // уменьшение ордера покупателя
        {
            if (!tradeState.OrdersBuy.TryGetValue(itemId, out var order))
                return;

            order.Amount -= amount;
            if (order.Amount <= 0)
                tradeState.OrdersBuy.Remove(itemId); // Ордер покупателя закрыт.
            else
                tradeState.OrdersBuy[itemId] = order; // Частичное исполнение ордера.
        }

        private static bool TryForcedSell(
            ref Ship ship,
            int itemId,
            int requestedAmount,
            out int soldAmount,
            out int unitPrice)
        {
            soldAmount = 0;
            unitPrice = ResolveForcedSellPrice(itemId);

            int available = ship.Cargo.GetAmount(ItemType.Item, itemId);
            if (available <= 0)
                return false; // В трюме нет нужного товара.

            soldAmount = requestedAmount <= 0 ? available : (requestedAmount < available ? requestedAmount : available);
            if (soldAmount <= 0)
                return false;

            ship.Cargo.Remove(ItemType.Item, itemId, soldAmount); // Товар утилизируется, в склад станции не попадает.

            long total = (long)unitPrice * soldAmount;
            if (total > 0)
                ship.Owner.Money += total; // Фиксированная выручка по средней цене каталога.

            return true;
        }

        private static int ResolveForcedSellPrice(int itemId)
        {
            if (ItemCatalogService.TryGetInfo(ItemType.Item, itemId, out var info))
                return info.Price;

            return 0;
        }
    }
}
