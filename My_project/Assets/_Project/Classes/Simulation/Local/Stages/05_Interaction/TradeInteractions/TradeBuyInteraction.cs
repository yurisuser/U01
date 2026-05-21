using _Project.Scripts.Const;
using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using _Project.Scripts.Ships.Actions;
using _Project.Scripts.Simulation.Ships;
using _Project.Scripts.Stations;
using _Project.Scripts.Trade.Models;

namespace _Project.Scripts.Simulation.Local.Stages.Interaction
{
    /// <summary>Исполнение action TradeBuy на станции-цели.</summary>
    internal static class TradeBuyInteraction
    {
        public static void Process( // обработка покупки
            ref Ship ship,
            ref StarSys system,
            in Station targetStation,
            TradeModuleState tradeState)
        {
            var key = ship.CurrentAction.Key;
            if (!tradeState.OrdersSell.TryGetValue(key, out var order)) // ордер продажи отсутствует
            {
                TradeInteractionLogger.LogTradeOrderMissing("Sell", ship.Uid.Id, targetStation.Uid.Id, key);
                TradeInteractionHelpers.FailAndResetTrade(ref ship);
                return;
            }

            int amount = CalcBuyAmount(in ship, in targetStation, in order);
            if (amount <= 0) // нечего покупать
            {
                TradeInteractionHelpers.FailAndResetTrade(ref ship); // Сценарий потерял актуальность.
                return;
            }

            var offer = new TradeOffer(
                targetStation,
                ship,
                order.Key,
                amount,
                order.Price,
                system.Uid.Id);

            TradeInteractionLogger.LogTradeStart("Buy", ship.Uid.Id, targetStation.Uid.Id, order.Key, amount, order.Price);

            var result = _Project.Scripts.Trade.Services.TradeService.Execute(offer);
            if (!result.Success) // сделка не прошла
            {
                TradeInteractionLogger.LogTradeFailed("Buy", ship.Uid.Id, targetStation.Uid.Id, order.Key, amount, result.FailReason);
                if (result.FailReason == ETradeFailReason.NotEnoughCargoSpace) // нет места
                    TradeInteractionHelpers.DropCurrentTaskAndUndock(ref ship); // Пропускаем текущую buy-задачу и продолжаем стек.
                else
                    TradeInteractionHelpers.FailAndResetTrade(ref ship); // Для остальных ошибок лучше полный сброс.
                return;
            }

            ship.TaskState.Pop(); // Текущая buy-задача выполнена.
            HandlePartialBuy(ref ship, ref system, in order, amount); // Если купили не всё — пробуем найти следующего продавца.
            ApplyOrderSellDelta(ref tradeState, order.Key, amount); // Синхронизируем ордер станции-продавца.
            TradeInteractionHelpers.UndockSuccess(ref ship); // Возвращаем корабль в полет.
        }

        private static int CalcBuyAmount(in Ship ship, in Station targetStation, in OrderSell order) // расчет объема покупки
        {
            int amount = ship.CurrentAction.Amount;
            if (order.Amount < amount) // ограничение ордера
                amount = order.Amount;

            var sellerCargo = targetStation.Cargo;
            if (sellerCargo == null)
                return 0; // Нет грузового модуля/состояния.

            int available = sellerCargo.GetAmount(order.Key);
            if (available < amount) // ограничение склада
                amount = available; // Режем объем по фактическому остатку товара.

            return amount;
        }

        private static void HandlePartialBuy( // докупка остатка после частичной покупки
            ref Ship ship,
            ref StarSys system,
            in OrderSell order,
            int boughtAmount)
        {
            int requested = ship.CurrentAction.Amount;
            if (boughtAmount >= requested)
                return; // Полная покупка, допланирование не нужно.

            int remaining = requested - boughtAmount;
            if (_Project.Scripts.Trade.Services.SearchTradeService.TryFindBestSellerInSystem(system, order.Key, out var sellerUid) &&
                TradeInteraction.TryGetStation(in system, sellerUid, out var nextSeller))
            {
                // LIFO: пушим сначала торговое действие, затем move к найденному продавцу.
                ship.TaskState.PushTask(ShipTaskBuilder.TradeBuy(sellerUid, order.Key, remaining));
                ship.TaskState.PushTask(ShipTaskBuilder.MoveTo(
                    nextSeller.Position,
                    SimulationConsts.DestinationPointTolerance,
                    keepSpeed: true,
                    targetUid: sellerUid));
            }
        }


        private static void ApplyOrderSellDelta(ref TradeModuleState tradeState, _Project.Items.ItemKey key, int amount) // уменьшение ордера продавца
        {
            if (!tradeState.OrdersSell.TryGetValue(key, out var order))
                return;

            order.Amount -= amount;
            if (order.Amount <= 0)
                tradeState.OrdersSell.Remove(key); // Ордер закрыт.
            else
                tradeState.OrdersSell[key] = order; // Ордер остался частично активным.
        }
    }
}
