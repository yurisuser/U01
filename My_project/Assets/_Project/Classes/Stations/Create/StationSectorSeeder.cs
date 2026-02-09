using _Project.Scripts.Const;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.NPC.Fraction;
using _Project.Scripts.Galaxy.Config;

namespace _Project.Scripts.Stations
{
    /// <summary>Расставляет по одной станции фракции в каждом её секторе.</summary>
    public static class StationSectorSeeder
    {
        private static readonly StationTypeDef DefaultDef = new()
        {
            Key = "station_test",
            PrefabKey = "station_test",
            DefaultModules = new[] { EStationModuleType.Storage, EStationModuleType.Dock, EStationModuleType.Trade },
            BaseHull = 100f,
            BasePower = 50f,
        };

        public static void SpawnFactionStations(StarSys[] galaxy)
        {
            if (galaxy == null || galaxy.Length == 0)
                return;

            var fractions = FractionService.GetAll();
            for (int i = 0; i < fractions.Count; i++)
                SpawnForFraction(galaxy, fractions[i]);

            SpawnTemporaryExtraStations(galaxy, fractions);
        }

        private static void SpawnForFraction(StarSys[] galaxy, Fraction fraction)
        {
            if (fraction.HomeSector <= 0)
                return;

            var rng = new System.Random();
            for (int i = 0; i < galaxy.Length; i++)
            {
                ref var sys = ref galaxy[i];
                if (sys.ConstellationId != fraction.HomeSector)
                    continue;

                var station = StationSpawner.CreateForSystem(sys, DefaultDef, fraction);
                if (station.Modules == null || station.Modules.Length == 0)
                {
                    station.Modules = new[]
                    {
                        new StationModule
                        {
                            Type = EStationModuleType.Storage,
                            Level = 1,
                            Data = new StorageModuleData { Capacity = 100 },
                            State = new StorageModuleState()
                        },
                        new StationModule
                        {
                            Type = EStationModuleType.Dock,
                            Level = 1,
                            Data = new DockModuleData { Slots = 2, DockingRange = StarSysemConstants.PlanetOrbitUnit * 0.1f, Anchors = null },
                            State = new DockModuleState()
                        }
                        ,
                        new StationModule
                        {
                            Type = EStationModuleType.Trade,
                            Level = 1,
                            Data = new TradeModuleData(),
                            State = new TradeModuleState()
                        }
                    };
                }

                StationTradeBootstrap.InitForStation(ref station, rng);
                sys.Stations = new[] { station };
            }
        }

        /// <summary>ВРЕМЕННЫЙ спавн дополнительных станций (удалить безболезненно).</summary>
        private static void SpawnTemporaryExtraStations(StarSys[] galaxy, System.Collections.Generic.IReadOnlyList<Fraction> fractions)
        {
            if (fractions == null || fractions.Count == 0)
                return;

            var rng = new System.Random(1337);
            float orbitUnit = OrbitMath.PlanetOrbitIndexToUnits(1);
            float baseRadius = OrbitMath.PlanetOrbitIndexToUnits(10) + orbitUnit;

            for (int i = 0; i < galaxy.Length; i++)
            {
                ref var sys = ref galaxy[i];
                if (sys.Stations == null || sys.Stations.Length == 0)
                    continue;

                var ownerFraction = fractions[rng.Next(0, fractions.Count)];
                var station = StationCreator.Create(DefaultDef, ownerFraction, new UnityEngine.Vector3(0f, baseRadius, 0f)); // 12 часов

                StationTradeBootstrap.InitForStation(ref station, rng);
                AppendStation(ref sys, station);
            }
        }

        private static void AppendStation(ref StarSys sys, Station station)
        {
            if (sys.Stations == null || sys.Stations.Length == 0)
            {
                sys.Stations = new[] { station };
                return;
            }

            var expanded = new Station[sys.Stations.Length + 1];
            System.Array.Copy(sys.Stations, expanded, sys.Stations.Length);
            expanded[^1] = station;
            sys.Stations = expanded;
        }
    }
}
