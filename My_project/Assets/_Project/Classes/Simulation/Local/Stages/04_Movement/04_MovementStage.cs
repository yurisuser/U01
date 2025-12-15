using System.Collections.Generic;

namespace _Project.Scripts.Simulation.Local.Stages.Movement
{
    /// <summary>Оркестратор движения для всех динамических сущностей.</summary>
    public sealed class LocalMovementStage : ILocalSimulationStage
    {
        private readonly List<IMovementProcessor> _processors = new();

        public LocalMovementStage()
        {
            _processors.Add(new ShipMover());
            _processors.Add(new ProjectileMovementExecutor());
            _processors.Add(new DebrisMovementExecutor());
        }

        public void Run(in LocalSimulationContext context)
        {
            for (int i = 0; i < _processors.Count; i++)
                _processors[i].Run(in context);
        }
    }

    /// <summary>Заглушка для будущее обработки ракет/снарядов.</summary>
    public sealed class ProjectileMovementExecutor : IMovementProcessor
    {
        public void Run(in LocalSimulationContext context)
        {
            // На этом этапе логика не реализована.
        }
    }

    /// <summary>Заглушка для обработки обломков/мусора.</summary>
    public sealed class DebrisMovementExecutor : IMovementProcessor
    {
        public void Run(in LocalSimulationContext context)
        {
            // На этом этапе логика не реализована.
        }
    }
}
