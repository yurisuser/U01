using _Project.Scripts.Core.Runtime;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.PilotMotivation;
using _Project.Scripts.Simulation.Primitives;

namespace _Project.Scripts.Simulation.Behaviors
{
    internal static class AcquireTargetBehavior
    {
        public static BehaviorExecutionResult Execute(
            ref Ship ship,
            ref PilotMotive motive,
            in PilotAction action,
            StarSystemState state)
        {
            var acquire = action.Parameters.Acquire;

            if (!FindNearestHostilePrimitive.TryFind(state, in ship, acquire.SearchRadius, acquire.AllowFriendlyFire, out var snapshot, out _))
                return BehaviorExecutionResult.None;

            motive.SetCurrentTarget(snapshot.Uid);
            motive.CompleteCurrentAction();
            return BehaviorExecutionResult.CompletedResult;
        }
    }
}
