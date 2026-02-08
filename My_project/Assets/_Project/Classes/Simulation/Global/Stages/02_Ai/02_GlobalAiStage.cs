using _Project.Scripts.Ships;
using _Project.Scripts.Const;
using _Project.Scripts.Simulation.Core;
using _Project.Scripts.Simulation.Global.Debug;
using _Project.Scripts.Simulation.Ships;

namespace _Project.Scripts.Simulation.Global.Stages.Ai
{
    /// <summary>Глобальный AI: планирование задач для неактивных систем.</summary>
    public sealed class GlobalAiStage : ISimulationStage
    {
        public void Run(in SimulationStepContext context)
        {
            var gameState = context.GameState;
            var galaxy = gameState?.Galaxy;
            if (gameState == null || galaxy == null || galaxy.Length == 0)
                return; // Нет данных для обхода систем.

            GlobalFractionShipSpawner.EnsureShipsInFactionSystems(
                gameState,
                SimulationConsts.ShipsPerSystem,
                SimulationConsts.SpawnRadius); // Поддерживаем флот в фракционных системах.

            int activeSystemIndex = gameState.SelectedSystemIndex;
            for (int systemIndex = 0; systemIndex < galaxy.Length; systemIndex++)
            {
                if (systemIndex == activeSystemIndex)
                    continue; // Активная система тикается локальным пайплайном.

                var system = galaxy[systemIndex];
                var runtime = system.State;
                if (runtime == null)
                    continue; // В системе нет рантайма кораблей.

                var ships = runtime.Ships;
                for (int i = 0; i < ships.Count; i++)
                {
                    var ship = ships[i];
                    bool hadTasksBefore = ship.TaskState.HasTasks;
                    ShipTaskPlanner.EnsurePatrolTask(ref ship);
                    if (!hadTasksBefore && ship.TaskState.HasTasks)
                        GlobalTradeDebugProbe.LogShip(context.Day, systemIndex, in ship, "AI", "task stack initialized");
                    ships[i] = ship;
                }

                var probeBefore = GetTrackedProbeState(in system);
                TradeActionPlanner.EnsureTradeActionsForSystem(gameState, systemIndex, ref system);
                var probeAfter = GetTrackedProbeState(in system);
                if (probeBefore != probeAfter && probeAfter != null)
                    GlobalTradeDebugProbe.Log(context.Day, systemIndex, GlobalTradeDebugProbe.DebugShipUid, "AI", "trade planner changed tracked ship state");
                galaxy[systemIndex] = system;
            }
        }

        private static string GetTrackedProbeState(in _Project.Scripts.Galaxy.Data.StarSys system)
        {
            int trackedUid = GlobalTradeDebugProbe.DebugShipUid;
            if (trackedUid <= 0)
                return null;

            var runtime = system.State;
            if (runtime == null)
                return null;

            var ships = runtime.Ships;
            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                if (ship.Uid.Id != trackedUid)
                    continue;

                var topTask = ship.TaskState.TryPeek(out var task) ? task.Type.ToString() : "None";
                return "ship=" + ship.Uid.Id + ";action=" + ship.CurrentAction.Type + ";task=" + topTask + ";cargo=" + ship.Cargo.Used;
            }

            return null;
        }
    }
}
