using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships.Actions;
using _Project.Scripts.Stations;
using _Project.Scripts.Trade.Models;
using UnityEngine;

namespace _Project.Scripts.Simulation.Local.Stages.Interaction
{
    internal static class TradeInteraction
    {
        public static void ProcessTradeActions(ref StarSys system)
        {
            var stations = system.Stations;
            if (stations == null || stations.Length == 0)
                return;

            for (int s = 0; s < stations.Length; s++)
            {
                var station = stations[s];
                if (!TryGetDockState(in station, out var dockState))
                    continue;

                var docked = dockState.DockedShips;
                for (int i = 0; i < docked.Count; i++)
                {
                    var ship = docked[i];
                    if (ship.CurrentAction.Type != EShipActionType.TradeBuy &&
                        ship.CurrentAction.Type != EShipActionType.TradeSell)
                        continue;

                    if (!TryGetStation(in system, ship.CurrentAction.TargetUid, out var targetStation))
                    {
                        ship.CurrentAction = default;
                        ship.LastActionFailReason = EShipActionFailReason.TargetNotFound;
                        docked[i] = ship;
                        continue;
                    }

                    if (!TryGetTradeState(in targetStation, out var tradeState))
                    {
                        ship.CurrentAction = default;
                        ship.LastActionFailReason = EShipActionFailReason.TargetNotFound;
                        docked[i] = ship;
                        continue;
                    }

                    if (ship.CurrentAction.Type == EShipActionType.TradeBuy)
                    {
                        if (!tradeState.OrdersSell.TryGetValue(ship.CurrentAction.ItemId, out var order))
                        {
                            Debug.Log($"[Trade] Sell order missing. ship={ship.Uid.Id} station={targetStation.Uid.Id} item={ship.CurrentAction.ItemId}");
                            ship.CurrentAction = default;
                            docked[i] = ship;
                            continue;
                        }

                        int amount = ship.CurrentAction.Amount;
                        if (order.Amount < amount)
                            amount = order.Amount;

                        var offer = new TradeOffer(
                            targetStation,
                            ship,
                            _Project.Items.ItemType.Item,
                            order.ItemId,
                            amount,
                            order.Price,
                            system.Uid.Id);

                        Debug.Log($"[Trade] Buy start. ship={ship.Uid.Id} station={targetStation.Uid.Id} item={order.ItemId} amount={amount} price={order.Price}");

                        var result = _Project.Scripts.Trade.Services.TradeService.Execute(offer);
                        if (result.Success)
                        {
                            order.Amount -= amount;
                            if (order.Amount <= 0)
                                tradeState.OrdersSell.Remove(order.ItemId);
                            else
                                tradeState.OrdersSell[order.ItemId] = order;

                            ship.TaskState.Pop();
                            ship.CurrentAction = new ShipAction { Type = EShipActionType.Undock };
                            ship.LastActionFailReason = EShipActionFailReason.None;
                        }
                        else
                        {
                            Debug.Log($"[Trade] Buy failed. ship={ship.Uid.Id} station={targetStation.Uid.Id} item={order.ItemId} amount={amount} reason={result.FailReason}");
                            ship.CurrentAction = default;
                        }
                    }
                    else if (ship.CurrentAction.Type == EShipActionType.TradeSell)
                    {
                        if (!tradeState.OrdersBuy.TryGetValue(ship.CurrentAction.ItemId, out var order))
                        {
                            Debug.Log($"[Trade] Buy order missing. ship={ship.Uid.Id} station={targetStation.Uid.Id} item={ship.CurrentAction.ItemId}");
                            ship.CurrentAction = default;
                            docked[i] = ship;
                            continue;
                        }

                        int amount = ship.CurrentAction.Amount;
                        if (order.Amount < amount)
                            amount = order.Amount;

                        var offer = new TradeOffer(
                            ship,
                            targetStation,
                            _Project.Items.ItemType.Item,
                            order.ItemId,
                            amount,
                            order.Price,
                            system.Uid.Id);

                        Debug.Log($"[Trade] Sell start. ship={ship.Uid.Id} station={targetStation.Uid.Id} item={order.ItemId} amount={amount} price={order.Price}");

                        var result = _Project.Scripts.Trade.Services.TradeService.Execute(offer);
                        if (result.Success)
                        {
                            order.Amount -= amount;
                            if (order.Amount <= 0)
                                tradeState.OrdersBuy.Remove(order.ItemId);
                            else
                                tradeState.OrdersBuy[order.ItemId] = order;

                            ship.TaskState.Pop();
                            ship.CurrentAction = new ShipAction { Type = EShipActionType.Undock };
                            ship.LastActionFailReason = EShipActionFailReason.None;
                        }
                        else
                        {
                            Debug.Log($"[Trade] Sell failed. ship={ship.Uid.Id} station={targetStation.Uid.Id} item={order.ItemId} amount={amount} reason={result.FailReason}");
                            ship.CurrentAction = default;
                        }
                    }

                    docked[i] = ship;
                }
            }
        }

        private static bool TryGetTradeState(in Station station, out TradeModuleState tradeState)
        {
            var modules = station.Modules;
            if (modules != null)
            {
                for (int i = 0; i < modules.Length; i++)
                {
                    var module = modules[i];
                    if (module == null || module.Type != EStationModuleType.Trade)
                        continue;

                    tradeState = module.State as TradeModuleState;
                    return tradeState != null;
                }
            }

            tradeState = null;
            return false;
        }

        private static bool TryGetStation(in StarSys system, UID stationUid, out Station station)
        {
            var stations = system.Stations;
            for (int i = 0; i < stations.Length; i++)
            {
                if (stations[i].Uid.Id == stationUid.Id)
                {
                    station = stations[i];
                    return true;
                }
            }

            station = default;
            return false;
        }

        private static bool TryGetDockState(in Station station, out DockModuleState dockState)
        {
            var modules = station.Modules;
            if (modules == null)
            {
                dockState = null;
                return false;
            }

            for (int i = 0; i < modules.Length; i++)
            {
                var module = modules[i];
                if (module == null || module.Type != EStationModuleType.Dock)
                    continue;

                dockState = module.State as DockModuleState;
                return dockState != null;
            }

            dockState = null;
            return false;
        }
    }
}
