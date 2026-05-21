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
            var key = ship.CurrentAction.Key;
            if (!tradeState.OrdersBuy.TryGetValue(key, out var order)) // ордер покупки отсутствует
            {
                TradeInteractionLogger.LogTradeOrderMissing("Buy", ship.Uid.Id, targetStation.Uid.Id, key);
                if (TryForcedSell(ref ship, key, ship.CurrentAction.Amount, out var soldAmount, out var unitPrice))
                {
                    TradeInteractionLogger.LogTradeForcedSell(ship.Uid.Id, targetStation.Uid.Id, key, soldAmount, unitPrice);
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
                order.Key,
                amount,
                order.Price,
                system.Uid.Id);

            TradeInteractionLogger.LogTradeStart("Sell", ship.Uid.Id, targetStation.Uid.Id, order.Key, amount, order.Price);

            var result = _Project.Scripts.Trade.Services.TradeService.Execute(offer);
            if (!result.Success) // сделка не прошла
            {
                TradeInteractionLogger.LogTradeFailed("Sell", ship.Uid.Id, targetStation.Uid.Id, order.Key, amount, result.FailReason);
                TradeInteractionHelpers.FailAndResetTrade(ref ship); // Ошибку продажи не лечим частично.
                return;
            }

            ApplyOrderBuyDelta(ref tradeState, order.Key, amount); // Уменьшаем встречный buy-ордер станции.
            ship.TaskState.Pop();                                     // Sell-задача исполнена.
            TradeInteractionHelpers.UndockSuccess(ref ship);          // Возвращаем корабль в полет.
        }

        private static int CalcSellAmount(in Ship ship, in OrderBy order) // расчет объема продажи
        {
            int amount = ship.CurrentAction.Amount;
            if (order.Amount < amount) // ограничение ордера
                amount = order.Amount;

            int available = ship.Cargo.GetAmount(order.Key);
            if (available < amount) // ограничение трюма
                amount = available; // Нельзя продать больше, чем есть в трюме.

            return amount;
        }

        private static void ApplyOrderBuyDelta(ref TradeModuleState tradeState, ItemKey key, int amount) // уменьшение ордера покупателя
        {
            if (!tradeState.OrdersBuy.TryGetValue(key, out var order))
                return;

            order.Amount -= amount;
            if (order.Amount <= 0)
                tradeState.OrdersBuy.Remove(key); // Ордер покупателя закрыт.
            else
                tradeState.OrdersBuy[key] = order; // Частичное исполнение ордера.
        }

        private static bool TryForcedSell(
            ref Ship ship,
            ItemKey key,
            int requestedAmount,
            out int soldAmount,
            out int unitPrice)
        {
            soldAmount = 0;
            unitPrice = ResolveForcedSellPrice(key);

            int available = ship.Cargo.GetAmount(key);
            if (available <= 0)
                return false; // В трюме нет нужного товара.

            soldAmount = requestedAmount <= 0 ? available : (requestedAmount < available ? requestedAmount : available);
            if (soldAmount <= 0)
                return false;

            ship.Cargo.Remove(key, soldAmount); // Товар утилизируется, в склад станции не попадает.

            long total = (long)unitPrice * soldAmount;
            if (total > 0)
                ship.Owner.Money += total; // Фиксированная выручка по средней цене каталога.

            return true;
        }

        private static int ResolveForcedSellPrice(ItemKey key)
        {
            if (ItemCatalogService.TryGetInfo(key.Type, key.Id, out var info))
                return info.Price;

            return 0;
        }
    }
}
