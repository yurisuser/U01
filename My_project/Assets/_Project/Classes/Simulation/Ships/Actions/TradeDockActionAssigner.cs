using _Project.Scripts.Ships;
using _Project.Scripts.Ships.Actions;

namespace _Project.Scripts.Simulation.Ships
{
    /// <summary>Общий helper: перевод верхней MoveToPoint(target station) задачи в Dock-action.</summary>
    internal static class TradeDockActionAssigner
    {
        public static bool TryAssignFromTopMoveTask(ref Ship ship)
        {
            if (!ship.CurrentAction.IsEmpty)
                return false; // Уже есть активное действие.

            if (!ship.TaskState.TryPeek(out var task) ||
                task.Type != EShipTaskType.MoveToPoint ||
                task.Params.MoveToPointParams.TargetUid.Id == 0)
            {
                return false; // Верх стека не привязан к станции.
            }

            ship.CurrentAction = new ShipAction
            {
                Type = EShipActionType.Dock,
                TargetUid = task.Params.MoveToPointParams.TargetUid,
            };

            return true;
        }
    }
}
