namespace _Project.Scripts.Simulation.AI
{
    /// <summary>Неизменяемый исход конкретной задачи.</summary>
    public readonly struct ShipAiTaskResult
    {
        public ShipAiTaskResult(EShipAiTaskOutcome outcome, int amount = 0)
        {
            Outcome = outcome;
            Amount = amount;
        }

        public EShipAiTaskOutcome Outcome { get; }
        public int Amount { get; }
    }
}
