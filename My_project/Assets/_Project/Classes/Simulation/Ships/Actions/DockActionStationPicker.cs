using _Project.Scripts.Ships;
using _Project.Scripts.Stations;

namespace _Project.Scripts.Simulation.Ships
{
    /// <summary>Выбор станции для дока по стабильному индексу корабля.</summary>
    internal static class DockActionStationPicker
    {
        public static Station PickStationForShip(in Ship ship, Station[] stations)
        {
            int index = ship.Uid.Id;
            if (index < 0)
                index = -index; // Нормализуем отрицательный UID.

            return stations[index % stations.Length]; // Один и тот же ship обычно получит ту же станцию.
        }
    }
}
