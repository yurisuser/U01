using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Stations;

namespace _Project.Scripts.Simulation.Ships
{
    /// <summary>Единый поиск станции по UID для торговых планнеров.</summary>
    internal static class TradePlannerStationResolver
    {
        public static bool TryGetStation(in StarSys system, UID stationUid, out Station station)
        {
            var stations = system.Stations;
            if (stations != null)
            {
                for (int i = 0; i < stations.Length; i++)
                {
                    if (stations[i].Uid.Id == stationUid.Id)
                    {
                        station = stations[i]; // Возвращаем копию value-type станции из массива системы.
                        return true;
                    }
                }
            }

            station = default; // Явный fail-path для вызывающего планнера.
            return false;
        }
    }
}
