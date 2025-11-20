namespace _Project.Scripts.Simulation.Behaviors
{
    /// <summary>Результат выполнения поведения пилота.</summary>
    internal readonly struct BehaviorExecutionResult
    {
        public readonly bool Completed; // Действие завершено.
        public readonly bool TargetLost; // Цель потеряна и была очищена.

        private BehaviorExecutionResult(bool completed, bool targetLost)
        {
            Completed = completed;
            TargetLost = targetLost;
        }

        public static BehaviorExecutionResult None => new BehaviorExecutionResult(false, false); // Продолжаем выполнять.

        public static BehaviorExecutionResult CompletedResult => new BehaviorExecutionResult(true, false); // Успех.

        public static BehaviorExecutionResult TargetLostResult => new BehaviorExecutionResult(true, true); // Цель пропала.
    }
}
