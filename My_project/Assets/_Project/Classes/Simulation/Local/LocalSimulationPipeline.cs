using System.Collections.Generic;
using _Project.Scripts.Simulation.Core;

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
            var localCtx = new LocalSimulationContext(
                context.GameState,
                context.GameState?.GetSelectedSystem(),
                context.Day,
                context.DeltaTime);

            for (int i = 0; i < _stages.Count; i++)
                _stages[i].Run(in localCtx);
        }
    }
}
