using _Project.Scripts.Ships;
using _Project.Scripts.Ships.Actions;
using _Project.Scripts.Simulation.Ships;

namespace _Project.Scripts.Simulation.Local.Stages.Interaction
{
    /// <summary>Стандартные завершения/сбросы торговых сценариев.</summary>
    internal static class TradeInteractionHelpers
    {
        internal static void FailAndResetTrade(ref Ship ship) // полный сброс торгового сценария
        {
            ship.TaskState = ShipTaskStack.Default; // Сбрасываем цепочку торговых задач целиком.
            UndockSuccess(ref ship);                // Освобождаем док и возвращаем в движение.
        }

        internal static void DropCurrentTaskAndUndock(ref Ship ship) // снять текущую задачу и выйти из дока
        {
            ship.TaskState.Pop();     // Точечный skip только верхней задачи.
            UndockSuccess(ref ship);  // После этого даем кораблю продолжить стек.
        }

        internal static void UndockSuccess(ref Ship ship) // успешное завершение торгового шага
        {
            ship.CurrentAction = new ShipAction { Type = EShipActionType.Undock }; // Interaction-стадия обработает отстыковку.
            ship.LastActionFailReason = EShipActionFailReason.None;                 // Сбрасываем флаг ошибки действия.
        }
    }
}
