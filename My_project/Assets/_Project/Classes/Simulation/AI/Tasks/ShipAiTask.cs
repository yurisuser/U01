namespace _Project.Scripts.Simulation.AI
{
    /// <summary>Неизменяемое описание задачи, выданной ИИ исполнителю.</summary>
    public abstract class ShipAiTask
    {
        protected ShipAiTask(EShipAiTaskType type)
        {
            Type = type;
        }

        public EShipAiTaskType Type { get; }
    }
}
