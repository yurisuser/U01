namespace _Project.Scripts.Simulation.AI
{
    /// <summary>Изменяемое состояние задачи, которым управляет исполнитель Simulation.</summary>
    public sealed class ShipAiTaskExecution
    {
        public ShipAiTaskExecution(ShipAiTask task)
        {
            Task = task;
            Status = EShipAiTaskStatus.Pending;
        }

        public ShipAiTask Task { get; }
        public EShipAiTaskStatus Status { get; private set; }
        public ShipAiTaskResult Result { get; private set; }

        public bool IsFinished => Status == EShipAiTaskStatus.Succeeded ||
                                  Status == EShipAiTaskStatus.Failed ||
                                  Status == EShipAiTaskStatus.Canceled ||
                                  Status == EShipAiTaskStatus.Rejected;

        public void Start()
        {
            if (Status == EShipAiTaskStatus.Pending || Status == EShipAiTaskStatus.Suspended)
                Status = EShipAiTaskStatus.Running;
        }

        public void Suspend()
        {
            if (Status == EShipAiTaskStatus.Pending || Status == EShipAiTaskStatus.Running)
                Status = EShipAiTaskStatus.Suspended;
        }

        public void Complete(EShipAiTaskOutcome outcome, int amount = 0)
        {
            if (IsFinished)
                return;

            Result = new ShipAiTaskResult(outcome, amount);
            Status = outcome switch
            {
                EShipAiTaskOutcome.Succeeded => EShipAiTaskStatus.Succeeded,
                EShipAiTaskOutcome.Failed => EShipAiTaskStatus.Failed,
                EShipAiTaskOutcome.Canceled => EShipAiTaskStatus.Canceled,
                EShipAiTaskOutcome.Rejected => EShipAiTaskStatus.Rejected,
                _ => EShipAiTaskStatus.Failed,
            };
        }
    }
}
