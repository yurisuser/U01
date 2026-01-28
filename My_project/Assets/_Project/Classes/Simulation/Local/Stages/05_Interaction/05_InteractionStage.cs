using _Project.Scripts.Const;
using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using _Project.Scripts.Ships.Actions;
using _Project.Scripts.Simulation.Ships;
using _Project.Scripts.Stations;

namespace _Project.Scripts.Simulation.Local.Stages.Interaction
{
    /// <summary>Локальные взаимодействия (стыковка/ремонт).</summary>
    public sealed class LocalInteractionStage : ILocalSimulationStage
    {
        public void Run(in LocalSimulationContext context)
        {
            if (!context.HasActiveSystem || context.GameState == null)
                return;

            var gameState = context.GameState;
            var galaxy = gameState.Galaxy;
            if (galaxy == null || galaxy.Length == 0)
                return;

            int index = gameState.SelectedSystemIndex;
            if (index < 0 || index >= galaxy.Length)
                return;

            var system = galaxy[index];
            if (system.State == null || system.Stations == null || system.Stations.Length == 0)
                return;

            ProcessDockActions(ref system);
            ProcessUndockActions(ref system);
            ProcessTradeActions(ref system);
            galaxy[index] = system;
        }

        private static void ProcessDockActions(ref StarSys system)
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
                ship.TaskState = ShipTaskStack.Default;
                ship.Position = station.Position;

                if (!dockState.Occupied.Contains(ship.Uid))
                    dockState.Occupied.Add(ship.Uid);
                dockState.DockedShips.Add(ship);

                ships.RemoveAt(i);
            }
        }

        private static void ProcessUndockActions(ref StarSys system)
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
                    ship.TaskState = ShipTaskStack.Default;
                    ship.Position = station.Position;

                    docked.RemoveAt(i);
                    dockState.Occupied.Remove(ship.Uid);
                    ships.Add(ship);
                }
            }
        }

        private static void ProcessTradeActions(ref StarSys system)
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

                    ship.CurrentAction = default;
                    ship.LastActionFailReason = EShipActionFailReason.None;
                    docked[i] = ship;
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
