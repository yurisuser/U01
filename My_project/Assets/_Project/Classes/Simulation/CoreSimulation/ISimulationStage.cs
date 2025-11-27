using _Project.Scripts.Core.GameState;

namespace _Project.Scripts.Simulation.Core
{
    /// <summary>Отдельная стадия симуляции.</summary>
    public interface ISimulationStage
    {
        /// <summary>Выполнить стадию для переданного контекста.</summary>
        void Run(in SimulationStepContext context);
    }
}
