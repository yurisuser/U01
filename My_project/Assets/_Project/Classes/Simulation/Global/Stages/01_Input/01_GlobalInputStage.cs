using _Project.Scripts.Simulation.Core;

namespace _Project.Scripts.Simulation.Global.Stages.Input
{
    /// <summary>Глобальный вход: при необходимости здесь применяются командные батчи игрока на ход.</summary>
    public sealed class GlobalInputStage : ISimulationStage
    {
        public void Run(in SimulationStepContext context)
        {
            // Точка расширения: обработка команд для удалённых систем.
        }
    }
}
