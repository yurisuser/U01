using _Project.Scripts.Const;
using _Project.Scripts.Ships;
using _Project.Scripts.Ships.Actions;
using _Project.Scripts.Stations;

namespace _Project.Scripts.Simulation.Ships
{
    /// <summary>Сборка минимального стека задач для захода на док.</summary>
    internal static class DockActionTaskComposer
    {
        public static void AssignDock(ref Ship ship, in Station station)
        {
            ship.CurrentAction = new ShipAction
            {
                Type = EShipActionType.Dock, // На interaction-стадии это триггерит DockingInteraction.
                TargetUid = station.Uid,     // Важен для валидации цели и расстояния.
            };

            ship.TaskState.PushTask(ShipTaskBuilder.MoveTo(
                station.Position,
                SimulationConsts.DestinationPointTolerance,
                keepSpeed: true)); // Двигаемся к станции без авто-торможения в ноль.
        }
    }
}
