using System.Collections.Generic;
using UnityEngine;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Simulation.Core;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Simulation;

namespace _Project.Scripts.Simulation.Local
{
    /// <summary>Конвейер локальных стадий для активной системы.</summary>
    public sealed class LocalSimulationPipeline : ISimulationPipeline
    {
        private readonly List<ILocalSimulationStage> _stages = new();

        public LocalSimulationPipeline()
        {
            _stages.Add(new Stages.LocalInputStage());
            _stages.Add(new Stages.LocalPerceptionStage());
            _stages.Add(new Stages.LocalAiStage());
            _stages.Add(new Stages.LocalMovementStage());
            _stages.Add(new Stages.LocalInteractionStage());
            _stages.Add(new Stages.LocalCombatStage());
            _stages.Add(new Stages.LocalEventsStage());
            _stages.Add(new Stages.LocalSnapshotStage());
        }

        public string Name => "Local";

        public void RunStep(in SimulationStepContext context)
        {
            var gameState = context.GameState;
            var localCtx = new LocalSimulationContext(
                gameState,
                gameState?.GetSelectedSystem(),
                context.Day,
                context.DeltaTime);

            var systemState = ResolveActiveState(gameState);
            systemState?.BeginShipSnapshot();

            for (int i = 0; i < _stages.Count; i++)
                _stages[i].Run(in localCtx);

            if (systemState != null)
                systemState.CommitShipSnapshot(localCtx.DeltaTime, Time.unscaledTime);
        }

        private static LocalSysRuntimeContext? ResolveActiveState(GameStateService gameState)
        {
            if (gameState == null)
                return null;

            var galaxy = gameState.Galaxy;
            if (galaxy == null || galaxy.Length == 0)
                return null;

            int index = gameState.SelectedSystemIndex;
            if (index < 0 || index >= galaxy.Length)
                return null;

            var system = galaxy[index];
            if (system.State == null)
            {
                system.State = new LocalSysRuntimeContext();
                galaxy[index] = system;
            }

            return system.State;
        }
    }
}
