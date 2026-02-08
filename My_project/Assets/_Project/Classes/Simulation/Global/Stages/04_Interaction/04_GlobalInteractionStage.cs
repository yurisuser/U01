using _Project.Scripts.Simulation.Core;
using _Project.Scripts.Simulation.Global.Debug;
using _Project.Scripts.Simulation.Local.Stages.Interaction;
using _Project.Scripts.Stations;

namespace _Project.Scripts.Simulation.Global.Stages.Interaction
{
    /// <summary>Глобальные взаимодействия: докинг, торговля и ан-док в неактивных системах.</summary>
    public sealed class GlobalInteractionStage : ISimulationStage
    {
        public void Run(in SimulationStepContext context)
        {
            var gameState = context.GameState;
            var galaxy = gameState?.Galaxy;
            if (gameState == null || galaxy == null || galaxy.Length == 0)
                return; // Нет данных для взаимодействий.

            int activeSystemIndex = gameState.SelectedSystemIndex;
            for (int systemIndex = 0; systemIndex < galaxy.Length; systemIndex++)
            {
                if (systemIndex == activeSystemIndex)
                    continue; // Активная система обрабатывается локально.

                var system = galaxy[systemIndex];
                if (system.State == null || system.Stations == null || system.Stations.Length == 0)
                    continue; // Нет контекста или станций для взаимодействия.

                var beforeDock = CaptureTrackedState(in system);
                DockingInteraction.ProcessDockActions(ref system);
                var afterDock = CaptureTrackedState(in system);
                TryLogChange(context.Day, systemIndex, "Dock", beforeDock, afterDock);

                var beforeTrade = afterDock;
                DockingInteraction.ProcessUndockActions(ref system);
                var afterUndock = CaptureTrackedState(in system);
                TryLogChange(context.Day, systemIndex, "Undock", beforeTrade, afterUndock);

                var beforeInteraction = afterUndock;
                TradeInteraction.ProcessTradeActions(ref system);
                var afterInteraction = CaptureTrackedState(in system);
                TryLogChange(context.Day, systemIndex, "Trade", beforeInteraction, afterInteraction);

                galaxy[systemIndex] = system;
            }
        }

        private static void TryLogChange(int day, int systemIndex, string stage, string before, string after)
        {
            if (before == after || string.IsNullOrEmpty(after))
                return; // Изменений нет или корабль не найден.

            GlobalTradeDebugProbe.Log(day, systemIndex, GlobalTradeDebugProbe.DebugShipUid, stage, "state changed: " + after);
        }

        private static string CaptureTrackedState(in _Project.Scripts.Galaxy.Data.StarSys system)
        {
            int trackedUid = GlobalTradeDebugProbe.DebugShipUid;
            if (trackedUid <= 0)
                return null;

            var runtime = system.State;
            if (runtime != null)
            {
                var ships = runtime.Ships;
                for (int i = 0; i < ships.Count; i++)
                {
                    var ship = ships[i];
                    if (ship.Uid.Id != trackedUid)
                        continue;

                    var topTask = ship.TaskState.TryPeek(out var task) ? task.Type.ToString() : "None";
                    return "loc=space;action=" + ship.CurrentAction.Type + ";task=" + topTask + ";cargo=" + ship.Cargo.Used;
                }
            }

            var stations = system.Stations;
            if (stations == null)
                return null;

            for (int s = 0; s < stations.Length; s++)
            {
                if (!TryGetDockState(in stations[s], out var dockState))
                    continue;

                var docked = dockState.DockedShips;
                for (int i = 0; i < docked.Count; i++)
                {
                    var ship = docked[i];
                    if (ship.Uid.Id != trackedUid)
                        continue;

                    var topTask = ship.TaskState.TryPeek(out var task) ? task.Type.ToString() : "None";
                    return "loc=dock:" + stations[s].Uid.Id + ";action=" + ship.CurrentAction.Type + ";task=" + topTask + ";cargo=" + ship.Cargo.Used;
                }
            }

            return null;
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
