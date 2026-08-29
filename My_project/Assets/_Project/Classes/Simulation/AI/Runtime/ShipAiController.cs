using _Project.Scripts.Ships;
using _Project.Scripts.Galaxy.Data;

namespace _Project.Scripts.Simulation.AI
{
    /// <summary>Продвигает поведения корабля до ожидания результата текущей задачи.</summary>
    public static class ShipAiController
    {
        public static void Advance(ref Ship ship, in StarSys system)
        {
            var runtime = ship.Ai;
            if (runtime == null)
                return;

            if (runtime.TaskExecution != null && runtime.TaskExecution.IsFinished)
            {
                var result = runtime.TaskExecution.Result;
                if (runtime.Behaviors.TryPeek(out var completedBehavior) &&
                    completedBehavior.IsCompletedBy(in result))
                {
                    runtime.Behaviors.Pop();
                    runtime.CurrentOrder = default;
                }

                runtime.TaskExecution = null;
            }

            if (runtime.TaskExecution != null || !EnsureRootBehavior(runtime, ship.Uid.Id))
                return;

            if (!runtime.Behaviors.TryPeek(out var behavior) || !behavior.TryCreateTask(in ship, in system, out var task))
                return;

            runtime.TaskExecution = new ShipAiTaskExecution(task);
        }

        private static bool EnsureRootBehavior(ShipAiRuntime runtime, int shipId)
        {
            if (!runtime.Behaviors.IsEmpty)
                return true;

            if (runtime.CurrentOrder.IsEmpty)
                return false;

            switch (runtime.CurrentOrder.Type)
            {
                case EShipAiOrderType.MoveToPoint:
                    runtime.Behaviors.Push(new MoveToPointBehavior(
                        runtime.CurrentOrder.Destination,
                        runtime.CurrentOrder.Tolerance,
                        runtime.CurrentOrder.KeepSpeed));
                    return true;

                case EShipAiOrderType.Patrol:
                    runtime.Behaviors.Push(new PatrolBehavior(
                        runtime.CurrentOrder.Center,
                        runtime.CurrentOrder.Radius,
                        runtime.CurrentOrder.Tolerance,
                        shipId));
                    return true;

                case EShipAiOrderType.TradeInSystem:
                    runtime.Behaviors.Push(new TradeInSystemBehavior(runtime.CurrentOrder.Tolerance));
                    return true;

                default:
                    return false;
            }
        }
    }
}
