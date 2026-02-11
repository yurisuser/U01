using _Project.Scripts.Simulation.Core;
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

            int activeSystemIndex = context.ActiveSystemIndex;
            GlobalFractionShipSpawner.EnsureShipsInFactionSystems(
                gameState,
                _Project.Scripts.Const.SimulationConsts.ShipsPerSystem,
                _Project.Scripts.Const.SimulationConsts.SpawnRadius,
                activeSystemIndex); // Поддерживаем флот во всех фракционных системах, кроме active.

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
                    ShipTaskPlanner.EnsurePatrolTask(ref ship);
                    ships[i] = ship;
                }

                TradeActionPlanner.EnsureTradeActionsForSystem(gameState, systemIndex, ref system);
                galaxy[systemIndex] = system;
            }
        }
    }
}
