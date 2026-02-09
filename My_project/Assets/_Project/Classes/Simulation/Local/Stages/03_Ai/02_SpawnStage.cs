using _Project.Scripts.Const;

namespace _Project.Scripts.Simulation.Local.Stages.Ai
{
    /// <summary>Локальный тестовый спавн кораблей для активной системы.</summary>
    public sealed class LocalSpawnStage : ILocalSimulationStage
    {
        public void Run(in LocalSimulationContext context)
        {
            // Временный баланс: поддерживаем целевое число кораблей в активной системе.
            LocalTestShipSpawner.RunPatrolPrototype(
                in context,
                SimulationConsts.ShipsPerSystem,
                SimulationConsts.SpawnRadius);
        }
    }
}
