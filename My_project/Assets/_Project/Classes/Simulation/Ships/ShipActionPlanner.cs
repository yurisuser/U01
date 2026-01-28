using _Project.Scripts.Const;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using _Project.Scripts.Ships.Actions;
using _Project.Scripts.Simulation.Local;
using _Project.Scripts.Stations;

namespace _Project.Scripts.Simulation.Ships
{
    public static class ShipActionPlanner
    {
        public static void EnsureDockActions(in LocalSimulationContext context)
        {
            if (!context.HasActiveSystem)
                return;

            var system = context.ActiveSystem.Value;
            var stations = system.Stations;
            if (stations == null || stations.Length == 0)
                return;

            var runtime = system.State;
            if (runtime == null)
                return;

            var ships = runtime.Ships;
            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                if (!ship.CurrentAction.IsEmpty)
                    continue;

                var station = PickStationForShip(in ship, stations);
                ship.CurrentAction = new ShipAction
                {
                    Type = EShipActionType.Dock,
                    TargetUid = station.Uid,
                };
                ship.TaskState.PushTask(ShipTask.MoveTo(
                    station.Position,
                    SimulationConsts.DestinationPointTolerance,
                    keepSpeed: true));
                ships[i] = ship;
            }
        }

        private static Station PickStationForShip(in Ship ship, Station[] stations)
        {
            int index = ship.Uid.Id;
            if (index < 0)
                index = -index;
            return stations[index % stations.Length];
        }
    }
}
