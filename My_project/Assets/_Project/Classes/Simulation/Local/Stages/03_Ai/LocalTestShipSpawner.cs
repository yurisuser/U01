using UnityEngine;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.NPC.Fraction;
using _Project.Scripts.Core;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation;
using _Project.Scripts.Simulation.Local;
using _Project.Scripts.Simulation.Ships;
using _Project.Scripts.Stations;

namespace _Project.Scripts.Simulation.Local.Stages.Ai
{
    /// <summary>Временный спавнер кораблей для локального теста.</summary>
    public static class LocalTestShipSpawner
    {
        public static void RunPatrolPrototype(in LocalSimulationContext context, int shipTarget, float patrolRadius)
        {
            var runtime = PrepareRuntime(in context, shipTarget, patrolRadius);
            if (runtime == null)
                return;

            var ships = runtime.Ships;
            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                if (ship.TopOrder.IsEmpty)
                {
                    ship.TopOrder = new _Project.Scripts.Ships.Orders.TopShipOrder
                    {
                        Type = _Project.Scripts.Ships.Orders.ETopShipOrderType.TradeInSystem,
                        Params = new _Project.Scripts.Ships.Orders.TopShipOrderParams
                        {
                            Center = Vector3.zero,
                            Radius = patrolRadius,
                            SystemIndex = context.GameState != null ? context.GameState.SelectedSystemIndex : -1
                        }
                    };
                }
                ships[i] = ship;
            }
        }

        private static LocalSysRuntimeContext PrepareRuntime(in LocalSimulationContext context, int shipTarget, float spawnRadius)
        {
            var gameState = context.GameState;
            if (gameState == null)
                return null;

            var galaxy = gameState.Galaxy;
            if (galaxy == null || galaxy.Length == 0)
                return null;

            int index = gameState.SelectedSystemIndex;
            if (index < 0 || index >= galaxy.Length)
                return null;

            var system = galaxy[index];
            var runtime = system.State;
            if (runtime == null)
            {
                runtime = new LocalSysRuntimeContext();
                system.State = runtime;
                galaxy[index] = system;
            }

            EnsureShipCount(runtime, system.Stations, shipTarget, spawnRadius);
            return runtime;
        }

        private static void EnsureShipCount(LocalSysRuntimeContext runtime, Station[] stations, int shipTarget, float spawnRadius)
        {
            var ships = runtime.Ships;
            int dockedCount = CountDockedShips(stations);
            int deficit = shipTarget - (ships.Count + dockedCount);
            if (deficit <= 0)
                return;

            for (int i = 0; i < deficit; i++)
            {
                var ship = SpawnShip();
                ship.Position = SamplePosition(spawnRadius);
                ship.Rotation = SampleOrientation();
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
                    if (module == null || module.Type != _Project.Scripts.Stations.EStationModuleType.Dock)
                        continue;

                    if (module.State is _Project.Scripts.Stations.DockModuleState dockState && dockState.DockedShips != null)
                        total += dockState.DockedShips.Count;
                }
            }

            return total;
        }

        private static Ship SpawnShip()
        {
            var fractions = FractionService.GetAll();
            Fraction fraction;
            if (fractions == null || fractions.Count == 0)
                fraction = new Fraction(0, "Default");
            else
            {
                int attempts = 0;
                fraction = fractions[Random.Range(0, fractions.Count)];
                while (fraction.FractionType == EFractionTypes.Player && attempts < fractions.Count) // избегаем фракции игрока для NPC
                {
                    int fracIndex = Random.Range(0, fractions.Count);
                    fraction = fractions[fracIndex];
                    attempts++;
                }
            }

            var pilotUid = UIDService.Create(EntityType.Individ);
            return ShipCreator.CreateShip(fraction, pilotUid);
        }

        private static Vector3 SamplePosition(float radius)
        {
            var offset = Random.insideUnitCircle * Mathf.Max(0f, radius);
            return new Vector3(offset.x, offset.y, 0f);
        }

        private static Quaternion SampleOrientation()
        {
            float yaw = Random.Range(0f, 360f);
            return Quaternion.Euler(0f, 0f, yaw);
        }
    }
}
