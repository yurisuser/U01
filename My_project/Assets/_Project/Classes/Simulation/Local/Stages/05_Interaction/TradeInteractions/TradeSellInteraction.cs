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
                if (TryReplanSell(in system, in ship, out var buyerUid, out var buyerStation)) // ищем другого покупателя
                {
                    ship.TaskState = ShipTaskStack.Default;
                    ship.TaskState.PushTask(ShipTaskBuilder.TradeSell(buyerUid, ship.CurrentAction.ItemId, ship.CurrentAction.Amount));
                    ship.TaskState.PushTask(ShipTaskBuilder.MoveTo(
                        buyerStation.Position,
                        SimulationConsts.DestinationPointTolerance,
                        keepSpeed: true,
                        targetUid: buyerUid));
                }
                else
                {
                    ship.TaskState = ShipTaskStack.Default;
                }

                TradeInteractionHelpers.UndockSuccess(ref ship);
                return;
            }

            int amount = CalcSellAmount(in ship, in order);
            if (amount <= 0) // нечего продавать
            {
                TradeInteractionHelpers.FailAndResetTrade(ref ship);
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
                TradeInteractionHelpers.FailAndResetTrade(ref ship);
                return;
            }

            ApplyOrderBuyDelta(ref tradeState, order.ItemId, amount);
            ship.TaskState.Pop();
            TradeInteractionHelpers.UndockSuccess(ref ship);
        }

        private static int CalcSellAmount(in Ship ship, in OrderBy order) // расчет объема продажи
        {
            int amount = ship.CurrentAction.Amount;
            if (order.Amount < amount) // ограничение ордера
                amount = order.Amount;

            int available = ship.Cargo.GetAmount(_Project.Items.ItemType.Item, order.ItemId);
            if (available < amount) // ограничение трюма
                amount = available;

            return amount;
        }

        private static void ApplyOrderBuyDelta(ref TradeModuleState tradeState, int itemId, int amount) // уменьшение ордера покупателя
        {
            if (!tradeState.OrdersBuy.TryGetValue(itemId, out var order))
                return;

            order.Amount -= amount;
            if (order.Amount <= 0)
                tradeState.OrdersBuy.Remove(itemId);
            else
                tradeState.OrdersBuy[itemId] = order;
        }

        private static bool TryReplanSell( // поиск нового покупателя
            in StarSys system,
            in Ship ship,
            out UID buyerUid,
            out Station buyerStation)
        {
            buyerUid = default;
            buyerStation = default;

            if (!_Project.Scripts.Trade.Services.SearchTradeService.TryFindBestBuyerInSystem(system, ship.CurrentAction.ItemId, out buyerUid))
                return false;

            return TradeInteraction.TryGetStation(in system, buyerUid, out buyerStation);
        }
    }
}
