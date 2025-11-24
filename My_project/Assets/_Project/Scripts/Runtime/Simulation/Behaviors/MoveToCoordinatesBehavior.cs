using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.PilotMotivation;
using _Project.Scripts.Simulation.Primitives;
using _Project.Scripts.Simulation.Render;

namespace _Project.Scripts.Simulation.Behaviors
{
    /// <summary>Поведение прямого перемещения к точке.</summary>
    internal static class MoveToCoordinatesBehavior
    {
        // Двигаем корабль и завершаем действие по достижению.
        public static BehaviorExecutionResult Execute(ref Ship ship, ref PilotMotive motive, in PilotAction action, float dt, ITraceSink traceSink)
        {
            var move = action.Parameters.Move;
            var reached = MoveToPosition.Execute(ref ship, move.Destination, move.DesiredSpeed, move.ArriveDistance, dt, stopOnArrival: true, traceSink: traceSink, traceUid: ship.Uid);
            if (reached)
            {
                motive.CompleteCurrentAction();
                return BehaviorExecutionResult.CompletedResult;
            }

            return BehaviorExecutionResult.None;
        }
    }
}
