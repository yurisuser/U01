using _Project.Scripts.Simulation;
using _Project.Scripts.Simulation.Ships;

namespace _Project.Scripts.Simulation.Local.Stages.Ai
{
    /// <summary>Тактическое принятие решений.</summary>
    public sealed class LocalAiStage : ILocalSimulationStage
    {
        public void Run(in LocalSimulationContext context)
        {
            ShipActionPlanner.EnsureTradeActions(in context); // Планируем торговые действия/стек задач.
            ShipActionPlanner.EnsureDockActions(in context);  // Для остальных кораблей добавляем базовый докинг.
        }
    }
}
