using UnityEngine;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation;
using _Project.Scripts.Stations;

namespace _Project.Scripts.Simulation.Local.Stages.Ai
{
    /// <summary>Поддержание целевого количества кораблей в локальной системе.</summary>
    internal static class LocalSpawnPopulation
    {
        public static void EnsureShipCount(LocalSysRuntimeContext runtime, Station[] stations, int shipTarget, float spawnRadius)
        {
            var ships = runtime.Ships;
            int dockedCount = CountDockedShips(stations);               // Считаем корабли, скрытые в доках.
            int deficit = shipTarget - (ships.Count + dockedCount);     // Сколько нужно досоздать.
            if (deficit <= 0)
                return;

            for (int i = 0; i < deficit; i++)
            {
                var ship = LocalSpawnShipFactory.CreateShip();
                ship.Position = SamplePosition(spawnRadius);     // Стартовая позиция в пределах радиуса.
                ship.Rotation = SampleOrientation();             // Случайный курс на старте.
                ships.Add(ship);
            }
        }

        private static int CountDockedShips(Station[] stations)
        {
            if (stations == null || stations.Length == 0)
                return 0;

            int total = 0;
            for (int i = 0; i < stations.Length; i++)
            {
                var modules = stations[i].Modules;
                if (modules == null)
                    continue;

                for (int m = 0; m < modules.Length; m++)
                {
                    var module = modules[m];
                    if (module == null || module.Type != EStationModuleType.Dock)
                        continue; // Учитываем только dock-модуль.

                    if (module.State is DockModuleState dockState && dockState.DockedShips != null)
                        total += dockState.DockedShips.Count; // DockedShips не входят в runtime.Ships.
                }
            }

            return total;
        }

        private static Vector3 SamplePosition(float radius)
        {
            var offset = Random.insideUnitCircle * Mathf.Max(0f, radius);
            return new Vector3(offset.x, offset.y, 0f); // Спаун в плоскости системы.
        }

        private static Quaternion SampleOrientation()
        {
            float yaw = Random.Range(0f, 360f);
            return Quaternion.Euler(0f, 0f, yaw);
        }
    }
}
