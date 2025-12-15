namespace _Project.Scripts.Simulation.Local.Stages.Movement
{
    /// <summary>Общий интерфейс для процессоров движения.</summary>
    public interface IMovementProcessor
    {
        void Run(in LocalSimulationContext context);
    }
}
