using _Project.Scripts.Simulation.Core;

namespace _Project.Scripts.Simulation.Global.Stages.Combat
{
    /// <summary>Глобальный бой: точка подключения дискретной боевой логики на ход.</summary>
    public sealed class GlobalCombatStage : ISimulationStage
    {
        public void Run(in SimulationStepContext context)
        {
            // Заглушка текущего этапа.
        }
    }
}
