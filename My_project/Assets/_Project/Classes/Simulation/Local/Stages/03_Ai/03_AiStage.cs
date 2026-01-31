using _Project.Scripts.Const;
using _Project.Scripts.Simulation;
using _Project.Scripts.Simulation.Ships;

namespace _Project.Scripts.Simulation.Local.Stages.Ai
{
    /// <summary>Тактическое принятие решений.</summary>
    public sealed class LocalAiStage : ILocalSimulationStage
    {
        public void Run(in LocalSimulationContext context)
        {
            LocalTestShipSpawner.RunPatrolPrototype(in context, SimulationConsts.ShipsPerSystem, SimulationConsts.SpawnRadius);
            ShipActionPlanner.EnsureTradeActions(in context);
            ShipActionPlanner.EnsureDockActions(in context);
        }
    }
}
