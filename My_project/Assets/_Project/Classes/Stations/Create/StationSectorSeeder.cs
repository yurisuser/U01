using System.Collections.Generic;
using _Project.Scripts.Const;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.NPC.Fraction;
using _Project.Scripts.Galaxy.Config;
using UnityEngine;

namespace _Project.Scripts.Stations
{
    /// <summary>Расставляет по одной станции фракции в каждом её секторе.</summary>
    public static class StationSectorSeeder
    {
        private const int MiningStationsPerSystem = 3;

        private static readonly StationTypeDef DefaultDef = new()
        {
            Key = "station_test",
            PrefabKey = "station_test",
            DefaultModules = new[] { EStationModuleType.Storage, EStationModuleType.Dock, EStationModuleType.Trade },
            BaseHull = 100f,
            BasePower = 50f,
        };

        private static readonly StationTypeDef MiningDef = new()
        {
            Key = "station_mining",
            PrefabKey = "station_test",
            DefaultModules = new[] { EStationModuleType.Storage, EStationModuleType.Dock, EStationModuleType.Industry },
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
            SpawnMiningStations(galaxy);
        }

        private static void SpawnMiningStations(StarSys[] galaxy)
        {
            for (int systemIndex = 0; systemIndex < galaxy.Length; systemIndex++)
            {
                ref var sys = ref galaxy[systemIndex];
                if (sys.OwnerFrac == null || sys.OwnerFrac.Id <= 0)
                    continue;

                var sources = CollectMiningSources(sys);
                int count = System.Math.Min(MiningStationsPerSystem, sources.Count);
                for (int i = 0; i < count; i++)
                {
                    var source = sources[i];
                    var position = FindMiningStationPosition(sys, source);
                    var station = StationCreator.Create(MiningDef, sys.OwnerFrac, position);
                    ConfigureMiningStation(ref station, source);
                    AppendStation(ref sys, station);
                }
            }
        }

        private static Vector3 FindMiningStationPosition(in StarSys sys, MiningSource source)
        {
            var obstacles = CollectCelestialPositions(sys);
            if (sys.Stations != null)
            {
                for (int i = 0; i < sys.Stations.Length; i++)
                    obstacles.Add(sys.Stations[i].Position);
            }

            if (source.IsMoon)
            {
                var direction = source.Center - source.ParentCenter;
                if (direction.sqrMagnitude <= 0.0001f)
                    direction = Vector3.right;
                return source.Center + direction.normalized * StarSysemConstants.MoonOrbitUnit;
            }

            float baseAngle = source.AnchorAngle;
            var hasFirstMoon = TryGetFirstMoonAngle(sys.PlanetSysArr[source.PlanetIndex], source.PlanetIndex, out var moonAngle);
            if (hasFirstMoon)
            {
                var plusPosition = source.Center + new Vector3(
                    Mathf.Cos(moonAngle + Mathf.PI / 3f),
                    Mathf.Sin(moonAngle + Mathf.PI / 3f),
                    0f) * source.OrbitRadius;
                var minusPosition = source.Center + new Vector3(
                    Mathf.Cos(moonAngle - Mathf.PI / 3f),
                    Mathf.Sin(moonAngle - Mathf.PI / 3f),
                    0f) * source.OrbitRadius;
                return ScorePosition(plusPosition, obstacles) >= ScorePosition(minusPosition, obstacles)
                    ? plusPosition
                    : minusPosition;
            }

            const int attempts = 32;
            float bestScore = float.MinValue;
            Vector3 bestPosition = source.Center + new Vector3(Mathf.Cos(baseAngle), Mathf.Sin(baseAngle), 0f) * source.OrbitRadius;
            for (int i = 0; i < attempts; i++)
            {
                float angle = baseAngle + i * Mathf.PI / 3f;
                var candidate = source.Center + new Vector3(
                    Mathf.Cos(angle) * source.OrbitRadius,
                    Mathf.Sin(angle) * source.OrbitRadius,
                    0f);

                float score = ScorePosition(candidate, obstacles);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPosition = candidate;
                }
            }

            return bestPosition;
        }

        private static bool TryGetFirstMoonAngle(PlanetSys planetSys, int planetIndex, out float angle)
        {
            if (planetSys.Moons != null)
            {
                for (int i = 0; i < planetSys.Moons.Length; i++)
                {
                    if (planetSys.Moons[i].OrbitIndex != 1)
                        continue;
                    angle = Hash01((planetIndex + 1) * 73856093 ^
                                   (i + 1) * 19349663 ^
                                   Mathf.Max(0, planetSys.Moons[i].OrbitIndex) * 83492791) * Mathf.PI * 2f;
                    return true;
                }
            }

            angle = 0f;
            return false;
        }

        private static List<Vector3> CollectCelestialPositions(in StarSys sys)
        {
            var positions = new List<Vector3> { Vector3.zero };
            if (sys.PlanetSysArr == null)
                return positions;

            for (int planetIndex = 0; planetIndex < sys.PlanetSysArr.Length; planetIndex++)
            {
                var planetSys = sys.PlanetSysArr[planetIndex];
                var planetPosition = GetPlanetPosition(planetSys);
                positions.Add(planetPosition);
                if (planetSys.Moons == null)
                    continue;

                for (int moonIndex = 0; moonIndex < planetSys.Moons.Length; moonIndex++)
                    positions.Add(planetPosition + GetMoonPosition(planetIndex, moonIndex, planetSys.Moons[moonIndex]));
            }

            return positions;
        }

        private static float ScorePosition(Vector3 candidate, List<Vector3> obstacles)
        {
            float minDistance = float.MaxValue;
            for (int i = 0; i < obstacles.Count; i++)
                minDistance = Mathf.Min(minDistance, Vector3.Distance(candidate, obstacles[i]));
            return minDistance;
        }

        private static Vector3 GetPlanetPosition(PlanetSys planetSys)
        {
            float radius = Mathf.Max(0, planetSys.OrbitIndex) * StarSysemConstants.PlanetOrbitUnit;
            return new Vector3(
                Mathf.Cos(planetSys.OrbitPosition) * radius,
                Mathf.Sin(planetSys.OrbitPosition) * radius,
                0f);
        }

        private static Vector3 GetMoonPosition(int planetIndex, int moonIndex, Moon moon)
        {
            float radius = Mathf.Max(0, moon.OrbitIndex) * StarSysemConstants.MoonOrbitUnit;
            float angle = Hash01((planetIndex + 1) * 73856093 ^
                                 (moonIndex + 1) * 19349663 ^
                                 Mathf.Max(0, moon.OrbitIndex) * 83492791) * Mathf.PI * 2f;
            return new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
        }

        private static float Hash01(int seed)
        {
            unchecked
            {
                uint x = (uint)seed;
                x ^= x >> 17; x *= 0xED5AD4BBu;
                x ^= x >> 11; x *= 0xAC4C1B51u;
                x ^= x >> 15; x *= 0x31848BABu;
                x ^= x >> 14;
                return (x & 0xFFFFFFu) / 16777216f;
            }
        }

        private static List<MiningSource> CollectMiningSources(in StarSys sys)
        {
            var sources = new List<MiningSource>(MiningStationsPerSystem);
            var resourceIds = new HashSet<int>();
            if (sys.PlanetSysArr == null)
                return sources;

            for (int planetIndex = 0; planetIndex < sys.PlanetSysArr.Length; planetIndex++)
            {
                var planetSys = sys.PlanetSysArr[planetIndex];
                AddDeposits(sys, planetSys.Planet.ResourceDeposits, planetIndex, -1, sources, resourceIds);

                if (planetSys.Moons == null)
                    continue;
                for (int moonIndex = 0; moonIndex < planetSys.Moons.Length; moonIndex++)
                    AddDeposits(sys, planetSys.Moons[moonIndex].ResourceDeposits, planetIndex, moonIndex, sources, resourceIds);
            }

            return sources;
        }

        private static void AddDeposits(
            in StarSys sys,
            ResourceDeposit[] deposits,
            int planetIndex,
            int moonIndex,
            List<MiningSource> sources,
            HashSet<int> resourceIds)
        {
            if (deposits == null)
                return;
            for (int i = 0; i < deposits.Length && sources.Count < MiningStationsPerSystem; i++)
            {
                var deposit = deposits[i];
                if (resourceIds.Add(deposit.ResourceId))
                {
                    sources.Add(new MiningSource
                    {
                        ResourceId = deposit.ResourceId,
                        DepositId = deposit.DepositId,
                        PlanetIndex = planetIndex,
                        MoonIndex = moonIndex,
                        Center = moonIndex < 0
                            ? GetPlanetPosition(sys.PlanetSysArr[planetIndex])
                            : GetPlanetPosition(sys.PlanetSysArr[planetIndex]) + GetMoonPosition(planetIndex, moonIndex, sys.PlanetSysArr[planetIndex].Moons[moonIndex]),
                        ParentCenter = GetPlanetPosition(sys.PlanetSysArr[planetIndex]),
                        IsMoon = moonIndex >= 0,
                        AnchorAngle = Hash01(deposit.ResourceId * 73856093 ^ deposit.DepositId * 19349663) * Mathf.PI * 2f,
                        OrbitRadius = moonIndex < 0
                            ? StarSysemConstants.PlanetOrbitUnit
                            : StarSysemConstants.MoonOrbitUnit
                    });
                }
            }
        }

        private static void ConfigureMiningStation(ref Station station, MiningSource source)
        {
            for (int i = 0; i < station.Modules.Length; i++)
            {
                var module = station.Modules[i];
                if (module == null || module.Type != EStationModuleType.Industry)
                    continue;
                if (module.State is IndustryModuleState state)
                {
                    state.ResourceId = source.ResourceId;
                    state.DepositId = source.DepositId;
                    state.SourcePlanetIndex = source.PlanetIndex;
                    state.SourceMoonIndex = source.MoonIndex;
                }
                return;
            }
        }

        private sealed class MiningSource
        {
            public int ResourceId;
            public int DepositId;
            public int PlanetIndex;
            public int MoonIndex;
            public Vector3 Center;
            public Vector3 ParentCenter;
            public bool IsMoon;
            public float AnchorAngle;
            public float OrbitRadius;
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
