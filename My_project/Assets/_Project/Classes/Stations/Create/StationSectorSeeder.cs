using _Project.Scripts.Const;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.NPC.Fraction;

namespace _Project.Scripts.Stations
{
    /// <summary>Расставляет по одной станции фракции в каждом её секторе.</summary>
    public static class StationSectorSeeder
    {
        private static readonly StationTypeDef DefaultDef = new()
        {
            Key = "station_test",
            PrefabKey = "station_test",
            DefaultModules = new[] { EStationModuleType.Cargo, EStationModuleType.Dock, EStationModuleType.Trade },
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
                            Type = EStationModuleType.Cargo,
                            Level = 1,
                            Data = new CargoModuleData { Capacity = 100 },
                            State = new CargoModuleState()
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
    }
}
