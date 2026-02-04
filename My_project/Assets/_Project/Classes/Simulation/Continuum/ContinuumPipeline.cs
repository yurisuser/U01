using _Project.Scripts.Simulation.Core;

namespace _Project.Scripts.Simulation.Continuum
{
    /// <summary>Глобальный пайплайн Continuum: тикает сервис в глобальной симуляции.</summary>
    public sealed class ContinuumPipeline : ISimulationPipeline
    {
        private readonly ContinuumService _service;

        public ContinuumPipeline(ContinuumService service)
        {
            _service = service;
        }

        public string Name => "Continuum";

        public void RunStep(in SimulationStepContext context)
        {
            _service?.Tick(in context);
        }
    }
}
