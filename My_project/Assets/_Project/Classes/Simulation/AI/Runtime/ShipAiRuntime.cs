namespace _Project.Scripts.Simulation.AI
{
    /// <summary>Сохраняемое состояние нового ИИ конкретного корабля.</summary>
    public sealed class ShipAiRuntime
    {
        public ShipAiOrder CurrentOrder;
        public readonly ShipAiBehaviorStack Behaviors = new ShipAiBehaviorStack();
        public ShipAiTaskExecution TaskExecution;

        public void ReplaceOrder(in ShipAiOrder order)
        {
            CurrentOrder = order;
            Behaviors.Clear();
            TaskExecution = null;
        }
    }
}
