using System.Collections.Generic;
using _Project.Scripts.Simulation.Core;
using _Project.Scripts.Simulation.Global.Debug;

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
            GlobalTradeDebugProbe.BeginTurn(context.Day, context.GameState); // Выбор корабля и старт лога хода.
            for (int i = 0; i < _stages.Count; i++)
                _stages[i].Run(in context);
            GlobalTradeDebugProbe.EndTurn(); // Сброс буфера лога в файл.
        }
    }
}
