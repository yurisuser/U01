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
            _stages.Add(new Stages.Input.LocalInputStage());
            _stages.Add(new Stages.Perception.LocalPerceptionStage());
            _stages.Add(new Stages.Ai.LocalSpawnStage());
            _stages.Add(new Stages.Ai.LocalAiStage());
            _stages.Add(new Stages.Movement.LocalMovementStage());
            _stages.Add(new Stages.Interaction.LocalInteractionStage());
            _stages.Add(new Stages.Combat.LocalCombatStage());
            _stages.Add(new Stages.Events.LocalEventsStage());
            _stages.Add(new Stages.Snapshot.LocalSnapshotStage());
        }

        public string Name => "Local";

        public void RunStep(in SimulationStepContext context)
        {
            var gameState = context.GameState;
            var localCtx = new LocalSimulationContext(
                gameState,
                gameState?.GetActiveLocalSystem(),
                context.Day,
                context.DeltaTime,
                context.IsTurnStart);

            var systemState = ResolveActiveState(gameState);
            systemState?.BeginShipSnapshot();

            for (int i = 0; i < _stages.Count; i++)
                _stages[i].Run(in localCtx);

            if (systemState != null)
                systemState.CommitShipSnapshot(localCtx.DeltaTime, Time.unscaledTime);
        }

        private static LocalSysRuntimeContext ResolveActiveState(GameStateService gameState)
        {
            if (gameState == null)
                return null;

            var galaxy = gameState.Galaxy;
            if (galaxy == null || galaxy.Length == 0)
                return null;

            int index = gameState.ActiveLocalSystemIndex;
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
