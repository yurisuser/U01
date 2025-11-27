namespace _Project.Scripts.Simulation.Core
{
    /// <summary>Пустой пайплайн для временной заглушки.</summary>
    public sealed class NoopSimulationPipeline : ISimulationPipeline
    {
        public string Name { get; }

        public NoopSimulationPipeline(string name = "Noop")
        {
            Name = string.IsNullOrWhiteSpace(name) ? "Noop" : name;
        }

        public void RunStep(in SimulationStepContext context)
        {
            // Ничего не делаем — заглушка.
        }
    }
}
