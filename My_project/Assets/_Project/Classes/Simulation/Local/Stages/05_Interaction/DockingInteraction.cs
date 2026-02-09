using _Project.Scripts.Const;
using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using _Project.Scripts.Ships.Actions;
using _Project.Scripts.Ships.Orders;
using _Project.Scripts.Simulation.Ships;
using _Project.Scripts.Stations;
using UnityEngine;

namespace _Project.Scripts.Simulation.Local.Stages.Interaction
{
    internal static class DockingInteraction
    {
        public static void ProcessDockActions(ref StarSys system)
        {
            var runtime = system.State;
            if (runtime == null)
                return;

            var ships = runtime.Ships;
            for (int i = ships.Count - 1; i >= 0; i--)
            {
                var ship = ships[i];
                if (ship.CurrentAction.Type != EShipActionType.Dock)
                    continue;

                if (!TryGetStation(in system, ship.CurrentAction.TargetUid, out var station))
                {
                    ship.CurrentAction = default;
                    ship.LastActionFailReason = EShipActionFailReason.TargetNotFound;
                    ships[i] = ship;
                    continue;
                }

                if (!IsAtTarget(in ship, in station))
                    continue;

                if (!TryGetDockState(in station, out var dockState))
                {
                    ship.CurrentAction = default;
                    ship.LastActionFailReason = EShipActionFailReason.DockModuleMissing;
                    ships[i] = ship;
                    continue;
                }

                ship.CurrentSpeed = 0f;
                ship.CurrentAction = default;
                ship.LastActionFailReason = EShipActionFailReason.None;
                if (ship.TopOrder.Type != ETopShipOrderType.TradeInSystem)
                    ship.TaskState = ShipTaskStack.Default;
                ship.Position = station.Position;

                Debug.Log($"[Dock] ship={ship.Uid.Id} station={station.Uid.Id}");

                if (!dockState.Occupied.Contains(ship.Uid))
                    dockState.Occupied.Add(ship.Uid);
                dockState.DockedShips.Add(ship);

                if (ship.TopOrder.Type == ETopShipOrderType.TradeInSystem &&
                    ship.TaskState.TryPeek(out var task))
                {
                    if (task.Type == EShipTaskType.MoveToPoint &&
                        task.Params.MoveToPointParams.TargetUid.Id == station.Uid.Id)
                    {
                        ship.TaskState.Pop();
                        ship.TaskState.TryPeek(out task);
                    }

                    if (task.Type == EShipTaskType.TradeBuy &&
                        task.Params.TradeBuyParams.StationUid.Id == station.Uid.Id)
                    {
                        ship.CurrentAction = new ShipAction
                        {
                            Type = EShipActionType.TradeBuy,
                            TargetUid = station.Uid,
                            ItemId = task.Params.TradeBuyParams.ItemId,
                            Amount = task.Params.TradeBuyParams.Amount
                        };
                    }
                    else if (task.Type == EShipTaskType.TradeSell &&
                        task.Params.TradeSellParams.StationUid.Id == station.Uid.Id)
                    {
                        ship.CurrentAction = new ShipAction
                        {
                            Type = EShipActionType.TradeSell,
                            TargetUid = station.Uid,
                            ItemId = task.Params.TradeSellParams.ItemId,
                            Amount = task.Params.TradeSellParams.Amount
                        };
                    }
                }

                ships.RemoveAt(i);
            }
        }

        public static void ProcessUndockActions(ref StarSys system)
        {
            var runtime = system.State;
            if (runtime == null)
                return;

            var ships = runtime.Ships;
            var stations = system.Stations;

            for (int s = 0; s < stations.Length; s++)
            {
                var station = stations[s];
                if (!TryGetDockState(in station, out var dockState))
                    continue;

                var docked = dockState.DockedShips;
                for (int i = docked.Count - 1; i >= 0; i--)
                {
                    var ship = docked[i];
                    if (ship.CurrentAction.Type != EShipActionType.Undock)
                        continue;

                    ship.CurrentSpeed = 0f;
                    ship.CurrentAction = default;
                    ship.LastActionFailReason = EShipActionFailReason.None;
                    if (ship.TopOrder.Type != ETopShipOrderType.TradeInSystem)
                        ship.TaskState = ShipTaskStack.Default;
                    ship.Position = station.Position;

                    Debug.Log($"[Undock] ship={ship.Uid.Id} station={station.Uid.Id}");

                    docked.RemoveAt(i);
                    dockState.Occupied.Remove(ship.Uid);
                    ships.Add(ship);
                }
            }
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

        private static bool IsAtTarget(in Ship ship, in Station station)
        {
            var delta = ship.Position - station.Position;
            float sqrDistance = delta.sqrMagnitude;
            float tolerance = SimulationConsts.DestinationPointTolerance;
            return sqrDistance <= tolerance * tolerance;
        }
    }
}
