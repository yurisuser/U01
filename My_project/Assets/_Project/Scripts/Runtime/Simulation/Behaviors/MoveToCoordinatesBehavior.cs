using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.PilotMotivation;
using _Project.Scripts.Simulation.Primitives;

namespace _Project.Scripts.Simulation.Behaviors
{
    internal static class MoveToCoordinatesBehavior
    {
        public static BehaviorExecutionResult Execute(ref Ship ship, ref PilotMotive motive, in PilotAction action, float dt)
        {
            var move = action.Parameters.Move;
            var reached = MoveToPosition.Execute(ref ship, move.Destination, move.DesiredSpeed, move.ArriveDistance, dt);
            if (reached)
            {
                motive.CompleteCurrentAction();
                return BehaviorExecutionResult.CompletedResult;
            }

            return BehaviorExecutionResult.None;
        }
    }
}
