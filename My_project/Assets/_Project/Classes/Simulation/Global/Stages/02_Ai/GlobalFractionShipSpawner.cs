using System;
using System.Threading;
using UnityEngine;
using SysRandom = System.Random;
using _Project.Scripts.Const;
using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.NPC.Fraction;
using _Project.Scripts.Ships;
using _Project.Scripts.Ships.Orders;
using _Project.Scripts.Simulation;
using _Project.Scripts.Simulation.Ships;
using _Project.Scripts.Stations;

namespace _Project.Scripts.Simulation.Global.Stages.Ai
{
    /// <summary>Глобальный спавнер: поддерживает флот в фракционных системах.</summary>
    internal static class GlobalFractionShipSpawner
    {
        private static readonly ThreadLocal<SysRandom> Rng = new ThreadLocal<SysRandom>(() =>
            new SysRandom(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)));

        public static void EnsureShipsInFactionSystems(GameStateService gameState, int shipTarget, float spawnRadius)
        {
            if (gameState == null)
                return; // Без game state нет доступа к галактике.

            var galaxy = gameState.Galaxy;
            if (galaxy == null || galaxy.Length == 0)
                return; // Пустая галактика — нечего поддерживать.

            for (int systemIndex = 0; systemIndex < galaxy.Length; systemIndex++)
            {
                var system = galaxy[systemIndex];
                if (!IsFactionSystem(in system))
                    continue; // Отсекаем нефракционные системы.

                var runtime = system.State;
                if (runtime == null)
                {
                    runtime = new LocalSysRuntimeContext();
                    system.State = runtime;
                }

                EnsureShipCount(runtime, system.Stations, shipTarget, system.OwnerFrac, systemIndex, spawnRadius);
                galaxy[systemIndex] = system;
            }
        }

        private static bool IsFactionSystem(in StarSys system)
        {
            return system.OwnerFrac != null && system.OwnerFrac.Id > 0;
        }

        private static void EnsureShipCount(
            LocalSysRuntimeContext runtime,
            Station[] stations,
            int shipTarget,
            Fraction ownerFraction,
            int systemIndex,
            float spawnRadius)
        {
            var ships = runtime.Ships;
            int dockedCount = CountDockedShips(stations);
            int deficit = shipTarget - (ships.Count + dockedCount);
            if (deficit <= 0)
                return; // Целевое количество уже достигнуто.

            for (int i = 0; i < deficit; i++)
            {
                var ship = SpawnShip(ownerFraction, systemIndex, spawnRadius);
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
                        continue;

                    if (module.State is DockModuleState dockState && dockState.DockedShips != null)
                        total += dockState.DockedShips.Count;
                }
            }

            return total;
        }

        private static Ship SpawnShip(Fraction ownerFraction, int systemIndex, float spawnRadius)
        {
            var pilotUid = UIDService.Create(EntityType.Individ);
            var ship = ShipCreator.CreateShip(ownerFraction, pilotUid);
            ship.Position = SamplePosition(spawnRadius);
            ship.Rotation = SampleOrientation();

            if (ship.TopOrder.IsEmpty)
            {
                ship.TopOrder = new TopShipOrder
                {
                    Type = ETopShipOrderType.TradeInSystem,
                    Params = new TopShipOrderParams
                    {
                        Center = Vector3.zero,
                        Radius = spawnRadius,
                        SystemIndex = systemIndex
                    }
                };
            }

            return ship;
        }

        private static Vector3 SamplePosition(float radius)
        {
            float r = Mathf.Max(0f, radius);
            var rng = Rng.Value;
            if (rng == null || r <= 0f)
                return Vector3.zero;

            // Равномерное распределение по кругу без UnityEngine.Random (worker-safe).
            double angle = rng.NextDouble() * Math.PI * 2d;
            double distance = Math.Sqrt(rng.NextDouble()) * r;
            float x = (float)(Math.Cos(angle) * distance);
            float y = (float)(Math.Sin(angle) * distance);
            return new Vector3(x, y, 0f);
        }

        private static Quaternion SampleOrientation()
        {
            var rng = Rng.Value;
            float yaw = rng == null ? 0f : (float)(rng.NextDouble() * 360d);
            return Quaternion.Euler(0f, 0f, yaw);
        }
    }
}
