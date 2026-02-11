using System.Collections.Generic;
using _Project.Scripts.Simulation.Core;

namespace _Project.Scripts.Simulation.Global
{
    /// <summary>Глобальный конвейер стадий: один прогон = один внутриигровой ход.</summary>
    public sealed class GlobalSimulationPipeline : ISimulationPipeline
    {
        private readonly List<ISimulationStage> _stages = new();

        public GlobalSimulationPipeline()
        {
            _stages.Add(new Stages.Input.GlobalInputStage());
            _stages.Add(new Stages.Ai.GlobalAiStage());
            _stages.Add(new Stages.Movement.GlobalMovementStage());
            _stages.Add(new Stages.Interaction.GlobalInteractionStage());
            _stages.Add(new Stages.Combat.GlobalCombatStage());
            _stages.Add(new Stages.Events.GlobalEventsStage());
        }

        public string Name => "Global";

        public void RunStep(in SimulationStepContext context)
        {
            for (int i = 0; i < _stages.Count; i++)
                _stages[i].Run(in context);
        }
    }
}
