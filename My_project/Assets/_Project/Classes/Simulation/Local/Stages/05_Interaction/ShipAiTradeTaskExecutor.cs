using _Project.Items;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using _Project.Scripts.Stations;
using _Project.Scripts.Simulation.AI;
using _Project.Scripts.Trade.Models;
using UnityEngine;

namespace _Project.Scripts.Simulation.Local.Stages.Interaction
{
    /// <summary>Исполняет новые задачи торговли после прибытия корабля к станции.</summary>
    internal static class ShipAiTradeTaskExecutor
    {
        public static void Process(ref StarSys system)
        {
            var runtime = system.State;
            if (runtime == null)
                return;

            var ships = runtime.Ships;
            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                var execution = ship.Ai?.TaskExecution;
                if (execution == null || execution.IsFinished || !(execution.Task is StationTradeTask task) ||
                    execution.Status == EShipAiTaskStatus.Suspended)
                    continue;

                if (Vector3.Distance(ship.Position, task.StationPosition) > task.Tolerance)
                    continue;

                ship.Position = task.StationPosition;
                ship.CurrentSpeed = 0f;

                if (!TryGetStation(in system, task.StationUid, out var station) ||
                    !TryGetTradeState(in station, out var tradeState))
                {
                    execution.Complete(EShipAiTaskOutcome.Failed);
                    ships[i] = ship;
                    continue;
                }

                if (task is BuyAtStationTask buyTask)
                    ExecuteBuy(ref ship, ref system, in station, tradeState, buyTask, execution);
                else if (task is SellAtStationTask sellTask)
                    ExecuteSell(ref ship, ref system, in station, tradeState, sellTask, execution);

                ships[i] = ship;
            }
        }

        private static void ExecuteBuy(ref Ship ship, ref StarSys system, in Station station, TradeModuleState tradeState, BuyAtStationTask task, ShipAiTaskExecution execution)
        {
            if (!tradeState.OrdersSell.TryGetValue(task.Key, out var order))
            {
                execution.Complete(EShipAiTaskOutcome.Succeeded, 0);
                return;
            }

            int amount = Min(task.Amount, order.Amount, station.Cargo?.GetAmount(task.Key) ?? 0, GetFreeCargo(in ship));
            if (amount <= 0)
            {
                execution.Complete(EShipAiTaskOutcome.Succeeded, 0);
                return;
            }

            var result = _Project.Scripts.Trade.Services.TradeService.Execute(
                new TradeOffer(station, ship, task.Key, amount, order.Price, system.Uid.Id));
            if (!result.Success)
            {
                execution.Complete(EShipAiTaskOutcome.Failed);
                return;
            }

            ApplySellDelta(tradeState, task.Key, amount);
            execution.Complete(EShipAiTaskOutcome.Succeeded, amount);
        }

        private static void ExecuteSell(ref Ship ship, ref StarSys system, in Station station, TradeModuleState tradeState, SellAtStationTask task, ShipAiTaskExecution execution)
        {
            if (!tradeState.OrdersBuy.TryGetValue(task.Key, out var order))
            {
                execution.Complete(EShipAiTaskOutcome.Succeeded, 0);
                return;
            }

            int amount = Min(task.Amount, order.Amount, ship.Cargo.GetAmount(task.Key));
            if (amount <= 0)
            {
                execution.Complete(EShipAiTaskOutcome.Succeeded, 0);
                return;
            }

            var result = _Project.Scripts.Trade.Services.TradeService.Execute(
                new TradeOffer(ship, station, task.Key, amount, order.Price, system.Uid.Id));
            if (!result.Success)
            {
                execution.Complete(EShipAiTaskOutcome.Failed);
                return;
            }

            ApplyBuyDelta(tradeState, task.Key, amount);
            execution.Complete(EShipAiTaskOutcome.Succeeded, amount);
        }

        private static int GetFreeCargo(in Ship ship)
        {
            return ship.Cargo.Capacity <= 0 ? int.MaxValue : Mathf.Max(0, ship.Cargo.Capacity - ship.Cargo.Used);
        }

        private static int Min(params int[] values)
        {
            int result = int.MaxValue;
            for (int i = 0; i < values.Length; i++)
                result = values[i] < result ? values[i] : result;
            return result;
        }

        private static void ApplySellDelta(TradeModuleState state, ItemKey key, int amount)
        {
            if (!state.OrdersSell.TryGetValue(key, out var order))
                return;
            order.Amount -= amount;
            if (order.Amount <= 0)
                state.OrdersSell.Remove(key);
            else
                state.OrdersSell[key] = order;
        }

        private static void ApplyBuyDelta(TradeModuleState state, ItemKey key, int amount)
        {
            if (!state.OrdersBuy.TryGetValue(key, out var order))
                return;
            order.Amount -= amount;
            if (order.Amount <= 0)
                state.OrdersBuy.Remove(key);
            else
                state.OrdersBuy[key] = order;
        }

        private static bool TryGetStation(in StarSys system, _Project.Scripts.Core.UID uid, out Station station)
        {
            if (system.Stations != null)
            {
                for (int i = 0; i < system.Stations.Length; i++)
                {
                    if (system.Stations[i].Uid.Id == uid.Id)
                    {
                        station = system.Stations[i];
                        return true;
                    }
                }
            }
            station = default;
            return false;
        }

        private static bool TryGetTradeState(in Station station, out TradeModuleState state)
        {
            if (station.Modules != null)
            {
                for (int i = 0; i < station.Modules.Length; i++)
                {
                    var module = station.Modules[i];
                    if (module != null && module.Type == EStationModuleType.Trade && module.State is TradeModuleState tradeState)
                    {
                        state = tradeState;
                        return true;
                    }
                }
            }
            state = null;
            return false;
        }
    }
}
