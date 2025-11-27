namespace _Project.Scripts.Simulation.Local
{
    /// <summary>Стадия локальной симуляции.</summary>
    public interface ILocalSimulationStage
    {
        void Run(in LocalSimulationContext context);
    }
}
