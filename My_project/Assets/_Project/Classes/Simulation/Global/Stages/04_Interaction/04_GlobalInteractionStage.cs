using _Project.Scripts.Simulation.Core;
using _Project.Scripts.Simulation.Local.Stages.Interaction;

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

            int activeSystemIndex = context.ActiveSystemIndex;
            for (int systemIndex = 0; systemIndex < galaxy.Length; systemIndex++)
            {
                if (systemIndex == activeSystemIndex)
                    continue; // Активная система обрабатывается локально.

                var system = galaxy[systemIndex];
                if (system.State == null || system.Stations == null || system.Stations.Length == 0)
                    continue; // Нет контекста или станций для взаимодействия.

                DockingInteraction.ProcessDockActions(ref system);
                DockingInteraction.ProcessUndockActions(ref system);
                TradeInteraction.ProcessTradeActions(ref system);

                galaxy[systemIndex] = system;
            }
        }
    }
}
