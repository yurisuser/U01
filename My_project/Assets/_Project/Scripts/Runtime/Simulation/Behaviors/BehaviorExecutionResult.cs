namespace _Project.Scripts.Simulation.Behaviors
{
    internal readonly struct BehaviorExecutionResult
    {
        public readonly bool Completed;
        public readonly bool TargetLost;

        private BehaviorExecutionResult(bool completed, bool targetLost)
        {
            Completed = completed;
            TargetLost = targetLost;
        }

        public static BehaviorExecutionResult None => new BehaviorExecutionResult(false, false);

        public static BehaviorExecutionResult CompletedResult => new BehaviorExecutionResult(true, false);

        public static BehaviorExecutionResult TargetLostResult => new BehaviorExecutionResult(true, true);
    }
}
