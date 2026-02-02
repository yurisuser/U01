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
    internal static class TradeBuyInteraction
    {
        public static void Process( // обработка покупки
            ref Ship ship,
            ref StarSys system,
            in Station targetStation,
            TradeModuleState tradeState)
        {
            if (!tradeState.OrdersSell.TryGetValue(ship.CurrentAction.ItemId, out var order)) // ордер продажи отсутствует
            {
                TradeInteractionLogger.LogTradeOrderMissing("Sell", ship.Uid.Id, targetStation.Uid.Id, ship.CurrentAction.ItemId);
                TradeInteractionHelpers.FailAndResetTrade(ref ship);
                return;
            }

            int amount = CalcBuyAmount(in ship, in targetStation, in order);
            if (amount <= 0) // нечего покупать
            {
                TradeInteractionHelpers.FailAndResetTrade(ref ship);
                return;
            }

            var offer = new TradeOffer(
                targetStation,
                ship,
                _Project.Items.ItemType.Item,
                order.ItemId,
                amount,
                order.Price,
                system.Uid.Id);

            TradeInteractionLogger.LogTradeStart("Buy", ship.Uid.Id, targetStation.Uid.Id, order.ItemId, amount, order.Price);

            var result = _Project.Scripts.Trade.Services.TradeService.Execute(offer);
            if (!result.Success) // сделка не прошла
            {
                TradeInteractionLogger.LogTradeFailed("Buy", ship.Uid.Id, targetStation.Uid.Id, order.ItemId, amount, result.FailReason);
                if (result.FailReason == ETradeFailReason.NotEnoughCargoSpace) // нет места
                    TradeInteractionHelpers.DropCurrentTaskAndUndock(ref ship);
                else
                    TradeInteractionHelpers.FailAndResetTrade(ref ship);
                return;
            }

            ship.TaskState.Pop();
            HandlePartialBuy(ref ship, ref system, in order, amount);
            ApplyOrderSellDelta(ref tradeState, order.ItemId, amount);
            TradeInteractionHelpers.UndockSuccess(ref ship);
        }

        private static int CalcBuyAmount(in Ship ship, in Station targetStation, in OrderSell order) // расчет объема покупки
        {
            int amount = ship.CurrentAction.Amount;
            if (order.Amount < amount) // ограничение ордера
                amount = order.Amount;

            var sellerCargo = targetStation.Cargo;
            if (sellerCargo == null)
                return 0;

            int available = sellerCargo.GetAmount(_Project.Items.ItemType.Item, order.ItemId);
            if (available < amount) // ограничение склада
                amount = available;

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
                return;

            int remaining = requested - boughtAmount;
            if (_Project.Scripts.Trade.Services.SearchTradeService.TryFindBestSellerInSystem(system, order.ItemId, out var sellerUid) &&
                TradeInteraction.TryGetStation(in system, sellerUid, out var nextSeller))
            {
                ship.TaskState.PushTask(ShipTask.TradeBuy(sellerUid, order.ItemId, remaining));
                ship.TaskState.PushTask(ShipTask.MoveTo(
                    nextSeller.Position,
                    SimulationConsts.DestinationPointTolerance,
                    keepSpeed: true,
                    targetUid: sellerUid));
            }
        }

        private static void ApplyOrderSellDelta(ref TradeModuleState tradeState, int itemId, int amount) // уменьшение ордера продавца
        {
            if (!tradeState.OrdersSell.TryGetValue(itemId, out var order))
                return;

            order.Amount -= amount;
            if (order.Amount <= 0)
                tradeState.OrdersSell.Remove(itemId);
            else
                tradeState.OrdersSell[itemId] = order;
        }
    }
}
