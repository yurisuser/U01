using _Project.Scripts.Ships;
using _Project.Scripts.Ships.Actions;
using _Project.Scripts.Simulation.Ships;

namespace _Project.Scripts.Simulation.Local.Stages.Interaction
{
    internal static class TradeInteractionHelpers
    {
        internal static void FailAndResetTrade(ref Ship ship) // полный сброс торгового сценария
        {
            ship.TaskState = ShipTaskStack.Default;
            UndockSuccess(ref ship);
        }

        internal static void DropCurrentTaskAndUndock(ref Ship ship) // снять текущую задачу и выйти из дока
        {
            ship.TaskState.Pop();
            UndockSuccess(ref ship);
        }

        internal static void UndockSuccess(ref Ship ship) // успешное завершение торгового шага
        {
            ship.CurrentAction = new ShipAction { Type = EShipActionType.Undock };
            ship.LastActionFailReason = EShipActionFailReason.None;
        }
    }
}
