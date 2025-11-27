namespace _Project.Scripts.Simulation.Core
{
    /// <summary>Пайплайн стадий симуляции.</summary>
    public interface ISimulationPipeline
    {
        /// <summary>Название пайплайна для логов/отладки.</summary>
        string Name { get; }

        /// <summary>Выполнить один шаг по переданному контексту.</summary>
        void RunStep(in SimulationStepContext context);
    }
}
