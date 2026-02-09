using _Project.Scripts.Ships;
using _Project.Scripts.Ships.Orders;
using _Project.Scripts.Simulation.Local;

namespace _Project.Scripts.Simulation.Ships
{
    /// <summary>Планнер докинга для кораблей без активного действия.</summary>
    internal static class DockActionPlanner
    {
        public static void EnsureDockActions(in LocalSimulationContext context)
        {
            if (!context.HasActiveSystem)
                return; // Нет активной системы — нечего планировать.

            var system = context.ActiveSystem.Value;
            var stations = system.Stations;
            if (stations == null || stations.Length == 0)
                return; // В системе нет станций для дока.

            var runtime = system.State;
            if (runtime == null)
                return; // Рантайм системы не подготовлен.

            var ships = runtime.Ships;
            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                if (ship.TopOrder.Type == ETopShipOrderType.TradeInSystem ||
                    ship.TopOrder.Type == ETopShipOrderType.TradeGalaxy)
                    continue; // Торговые top-order сами управляют докингом.
                if (!ship.CurrentAction.IsEmpty)
                    continue; // Не перезаписываем уже выбранное действие.

                var station = DockActionStationPicker.PickStationForShip(in ship, stations); // Детерминированно выбираем станцию.
                DockActionTaskComposer.AssignDock(ref ship, in station); // Ставим action + MoveTo в стек задач.
                ships[i] = ship;
            }
        }
    }
}
