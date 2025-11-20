using _Project.Scripts.Core.Runtime;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.PilotMotivation;
using _Project.Scripts.Simulation.Primitives;

namespace _Project.Scripts.Simulation.Behaviors
{
    /// <summary>Поведение выбора цели для пилота.</summary>
    internal static class ChoiceTargetBehavior
    {
        // Ищем ближайшую подходящую цель и назначаем её текущей.
        public static BehaviorExecutionResult Execute(
            ref Ship ship,
            ref PilotMotive motive,
            in PilotAction action,
            StarSystemState state)
        {
            var acquire = action.Parameters.Acquire;

            if (!TargetingPrimitive.TryFindNearestHostile(state, in ship, acquire.SearchRadius, acquire.AllowFriendlyFire, out var snapshot, out _))
                return BehaviorExecutionResult.None;

            motive.SetCurrentTarget(snapshot.Uid); // Сохраняем выбранную цель в мотиве.
            motive.CompleteCurrentAction(); // Отмечаем действие выполненным.
            return BehaviorExecutionResult.CompletedResult;
        }
    }
}
