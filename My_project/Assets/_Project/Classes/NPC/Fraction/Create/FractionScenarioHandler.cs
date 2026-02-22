using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using _Project.DataAccess;
using _Project.Scripts.Const;
using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Galaxy.Config;
using _Project.Scripts.Galaxy.Generation;
using _Project.Scripts.Stations;
using UnityEngine;

namespace _Project.Scripts.NPC.Fraction.Create
{
    /// <summary>
    /// Применяет сценарий заселения (home.scenario.json/json5) к системам фракции.
    /// </summary>
    public static class FractionScenarioHandler
    {
        private const float SecurityLevelHome = 1f;

        /// <summary>Точка входа для сценария конкретной фракции.</summary>
        public static void Apply(StarSys[] galaxy, Fraction fraction, CatalogFraction catalog)
        {
            if (galaxy == null || galaxy.Length == 0) return;
            if (string.IsNullOrWhiteSpace(catalog.DirectoryPath)) return;

            var scenarioPath = ResolveScenarioPath(catalog.DirectoryPath);
            if (scenarioPath == null) return; // нет сценария — ничего не делаем

            ScenarioDto scenario;
            try
            {
                scenario = ReadScenario(scenarioPath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Сценарий фракции не прочитан ({scenarioPath}): {ex.Message}");
                return;
            }

            var systemsInConstellation = CollectConstellationSystems(galaxy, fraction.HomeSector);
            if (systemsInConstellation.Count == 0) return;

            var distancesToExternal = ComputeDistanceToExternalHyper(galaxy, systemsInConstellation);
            var occupied = new HashSet<int>();

            var starNameIndexFromEnd = catalog.StarNames.Count - 1;
            var planetNameIndex = 0;
            var moonNameIndex = 0;
            int capitalIndex = -1;
            var configuredHomeSystemsCount = 0;
            var assignedHomeSystemsCount = 0;

            if (scenario.systems == null || scenario.systems.Length == 0)
                return;

            // Прогоняем сценарные системы по очереди
            foreach (var sysDto in scenario.systems)
            {
                var configuredSecurityLevel = ResolveSecurityLevel(sysDto, catalog.Name, scenarioPath);
                if (sysDto.isHome)
                    configuredHomeSystemsCount++;

                var targetIndex = PickSystem(galaxy, systemsInConstellation, sysDto.pick, occupied, distancesToExternal, capitalIndex);
                if (targetIndex == -1)
                {
                    Debug.LogWarning($"Сценарий фракции {catalog.Name}: не нашли свободную систему под pick={sysDto.pick}");
                    continue;
                }

                ref var sys = ref galaxy[targetIndex];
                ApplySystem(ref sys, sysDto, scenario.star, fraction, catalog,
                    ref starNameIndexFromEnd, ref planetNameIndex, ref moonNameIndex,
                    out var assignedIsHome, configuredSecurityLevel);
                occupied.Add(targetIndex);
                if (capitalIndex == -1 && string.Equals(sysDto.role, "capital", StringComparison.OrdinalIgnoreCase))
                    capitalIndex = targetIndex;

                if (assignedIsHome)
                    assignedHomeSystemsCount++;
            }

            if (configuredHomeSystemsCount != 1)
                throw new InvalidOperationException(
                    $"Сценарий фракции {catalog.Name} ({scenarioPath}) должен содержать ровно одну систему с isHome=true. Найдено: {configuredHomeSystemsCount}.");

            if (assignedHomeSystemsCount != 1)
                throw new InvalidOperationException(
                    $"Сценарий фракции {catalog.Name} ({scenarioPath}) не смог применить единственную систему isHome=true. Найдено применённых: {assignedHomeSystemsCount}.");
        }

        private static string ResolveScenarioPath(string dir)
        {
            var json5 = Path.Combine(dir, "home.scenario.json5");
            if (File.Exists(json5)) return json5;
            var json = Path.Combine(dir, "home.scenario.json");
            return File.Exists(json) ? json : null;
        }

        private static ScenarioDto ReadScenario(string path)
        {
            var raw = File.ReadAllText(path);
            // Убираем комментарии вида // ...
            raw = Regex.Replace(raw, @"//.*", string.Empty);
            var dto = JsonUtility.FromJson<ScenarioDto>(raw);
            if (dto == null) throw new InvalidOperationException("Parse returned null");
            return dto;
        }

        private static List<int> CollectConstellationSystems(StarSys[] galaxy, int constellationId)
        {
            var list = new List<int>();
            for (int i = 0; i < galaxy.Length; i++)
            {
                if (galaxy[i].ConstellationId == constellationId)
                    list.Add(i);
            }
            return list;
        }

        private static Dictionary<int, int> ComputeDistanceToExternalHyper(StarSys[] galaxy, List<int> indices)
        {
            var result = new Dictionary<int, int>();
            var visited = new HashSet<int>();
            var queue = new Queue<(int idx, int dist)>();

            // стартуем с внешних
            foreach (var i in indices)
            {
                if (HasExternalLink(galaxy, i))
                {
                    queue.Enqueue((i, 0));
                    visited.Add(i);
                }
            }

            while (queue.Count > 0)
            {
                var (idx, dist) = queue.Dequeue();
                result[idx] = dist;
                var links = galaxy[idx].links;
                if (links == null) continue;
                foreach (var l in links)
                {
                    if (l < 0 || l >= galaxy.Length) continue;
                    if (galaxy[l].ConstellationId != galaxy[idx].ConstellationId) continue;
                    if (visited.Add(l))
                        queue.Enqueue((l, dist + 1));
                }
            }

            // для тех, кто не достижим от внешних, ставим большое число
            foreach (var i in indices)
                if (!result.ContainsKey(i)) result[i] = int.MaxValue / 2;

            return result;
        }

        private static bool HasExternalLink(StarSys[] galaxy, int idx)
        {
            var sys = galaxy[idx];
            if (sys.links == null) return false;
            for (int i = 0; i < sys.links.Length; i++)
            {
                var target = sys.links[i];
                if (target < 0 || target >= galaxy.Length) continue;
                if (galaxy[target].ConstellationId != sys.ConstellationId)
                    return true;
            }
            return false;
        }

        private static int PickSystem(StarSys[] galaxy, List<int> indices, string pick, HashSet<int> occupied, Dictionary<int, int> distToExternal, int capitalIndex)
        {
            IEnumerable<int> candidates = indices.Where(i => !occupied.Contains(i));

            switch (pick)
            {
                case "closest_to_capital":
                {
                    var capitalIdx = capitalIndex != -1 ? capitalIndex : indices.FirstOrDefault();
                    var dist = ComputeDistancesFrom(galaxy, indices, capitalIdx);
                    candidates = candidates.OrderBy(i => dist[i]).ThenBy(i => i);
                    break;
                }
                case "far_to_capital":
                {
                    var capitalIdx = capitalIndex != -1 ? capitalIndex : indices.FirstOrDefault();
                    var dist = ComputeDistancesFrom(galaxy, indices, capitalIdx);
                    candidates = candidates.OrderByDescending(i => dist[i]).ThenBy(i => i);
                    break;
                }
                case "closest_to_hyper_distance":
                {
                    candidates = candidates.OrderBy(i => distToExternal.ContainsKey(i) ? distToExternal[i] : int.MaxValue).ThenBy(i => i);
                    break;
                }
                case "far_to_hyper_distance":
                {
                    candidates = candidates.OrderByDescending(i => distToExternal.ContainsKey(i) ? distToExternal[i] : -1).ThenBy(i => i);
                    break;
                }
                case "have_external_hyper":
                {
                    candidates = candidates.Where(i => HasExternalLink(galaxy, i));
                    break;
                }
                default:
                    return -1;
            }

            foreach (var c in candidates)
                return c;

            return -1;
        }

        private static Dictionary<int, int> ComputeDistancesFrom(StarSys[] galaxy, List<int> indices, int startIdx)
        {
            var result = new Dictionary<int, int>();
            var visited = new HashSet<int>();
            var queue = new Queue<(int idx, int dist)>();
            queue.Enqueue((startIdx, 0));
            visited.Add(startIdx);

            while (queue.Count > 0)
            {
                var (idx, dist) = queue.Dequeue();
                result[idx] = dist;
                var links = galaxy[idx].links;
                if (links == null) continue;
                foreach (var l in links)
                {
                    if (l < 0 || l >= galaxy.Length) continue;
                    if (galaxy[l].ConstellationId != galaxy[idx].ConstellationId) continue;
                    if (visited.Add(l))
                        queue.Enqueue((l, dist + 1));
                }
            }
            foreach (var i in indices)
                if (!result.ContainsKey(i)) result[i] = int.MaxValue / 2;
            return result;
        }

        private static void ApplySystem(ref StarSys sys, ScenarioSystem sysDto, ScenarioStar starDto, Fraction fraction, CatalogFraction catalog,
            ref int starNameIndexFromEnd, ref int planetNameIndex, ref int moonNameIndex,
            out bool assignedIsHome, float configuredSecurityLevel)
        {
            // звезда
            if (starDto != null)
            {
                var star = sys.Star;
                if (Enum.TryParse<EStarType>(starDto.type, true, out var st)) star.type = st;
                if (Enum.TryParse<EStarSize>(starDto.size, true, out var sz)) star.size = sz;
                if (catalog.StarNames != null && starNameIndexFromEnd >= 0 && starNameIndexFromEnd < catalog.StarNames.Count && starDto.catalogName)
                {
                    star.Name = catalog.StarNames[starNameIndexFromEnd];
                    starNameIndexFromEnd--;
                }
                sys.Star = star;
                sys.DisplayName = star.Name;
                sys.CustomName = star.Name;
            }

            // планеты/луны
            if (sysDto.planets != null && sysDto.planets.Length > 0)
            {
                var planetSysArr = new PlanetSys[sysDto.planets.Length];
                var planetOrbits = new int[sysDto.planets.Length];
                for (int i = 0; i < sysDto.planets.Length; i++)
                {
                    var pDto = sysDto.planets[i];
                    var planet = new Planet
                    {
                        Uid = Core.UIDService.Create(Core.EntityType.Planet),
                        Type = Enum.TryParse<EPlanetType>(pDto.type, true, out var pt) ? pt : EPlanetType.Stone,
                        isHome = pDto.isHome
                    };
                    planet.OrbitalDistance = 0;
                    planet.OrbitalPeriod = 0;
                    planet.Radius = pDto.radius;
                    planet.Name = (catalog.PlanetNames != null && planetNameIndex < catalog.PlanetNames.Count && pDto.catalogName)
                        ? catalog.PlanetNames[planetNameIndex++]
                        : planet.Name;

                    var moons = CreateMoons(pDto, catalog, ref moonNameIndex);

                    planetSysArr[i] = new PlanetSys
                    {
                        MotherStar = sys.Star,
                        OrbitIndex = pDto.orbit,
                        OrbitPosition = 0f,
                        Planet = planet,
                        Moons = moons
                    };
                    planetOrbits[i] = pDto.orbit;
                }
                sys.PlanetSysArr = planetSysArr;
                sys.PlanetOrbits = planetOrbits;
            }

            // станции
            sys.Stations = CreateStations(sysDto.stations, fraction, sys);

            if (sysDto.isHome) sys.isHome = true;
            sys.OwnerFrac = fraction;
            assignedIsHome = sysDto.isHome;
            sys.SecurityLevel = configuredSecurityLevel;
        }

        private static float ResolveSecurityLevel(ScenarioSystem sysDto, string fractionName, string scenarioPath)
        {
            // Берем уровень только из конфига. Если поле не задано, JsonUtility даст 0.
            float securityLevel = sysDto.securityLevel;

            if (securityLevel < 0f || securityLevel > 1f)
            {
                throw new InvalidOperationException(
                    $"securityLevel вне диапазона [0..1] у фракции {fractionName} ({scenarioPath}): {securityLevel}.");
            }

            if (securityLevel <= 0f)
            {
                throw new InvalidOperationException(
                    $"Для всех систем в сценарии securityLevel должен быть > 0 у фракции {fractionName} ({scenarioPath}).");
            }

            if (sysDto.isHome && !Mathf.Approximately(securityLevel, SecurityLevelHome))
            {
                throw new InvalidOperationException(
                    $"Для isHome системы securityLevel обязан быть {SecurityLevelHome} у фракции {fractionName} ({scenarioPath}).");
            }

            return securityLevel;
        }

        private static Moon[] CreateMoons(ScenarioPlanet pDto, CatalogFraction catalog, ref int moonNameIndex)
        {
            if (pDto.moons == null || pDto.moons.Length == 0) return Array.Empty<Moon>();
            var arr = new Moon[pDto.moons.Length];
            for (int i = 0; i < pDto.moons.Length; i++)
            {
                var mDto = pDto.moons[i];
                var moon = new Moon
                {
                    Uid = Core.UIDService.Create(Core.EntityType.Moon),
                    Type = Enum.TryParse<EMoonType>(mDto.type, true, out var mt) ? mt : EMoonType.Stone,
                    Size = Enum.TryParse<EMoonSize>(mDto.size, true, out var ms) ? ms : EMoonSize.Small,
                    OrbitIndex = mDto.orbit,
                    isHome = mDto.isHome
                };
                moon.Name = (catalog.MoonNames != null && moonNameIndex < catalog.MoonNames.Count && mDto.catalogName)
                    ? catalog.MoonNames[moonNameIndex++]
                    : moon.Name;
                arr[i] = moon;
            }
            return arr;
        }

        [Serializable]
        private class ScenarioDto
        {
            public ScenarioStar star;
            public ScenarioSystem[] systems;
        }

        [Serializable]
        private class ScenarioStar
        {
            public string type;
            public string size;
            public bool catalogName;
        }

        [Serializable]
        private class ScenarioSystem
        {
            public string role;
            public string pick;
            public bool isHome;
            public float securityLevel;
            public ScenarioPlanet[] planets;
            public ScenarioStation[] stations;
        }

        [Serializable]
        private class ScenarioPlanet
        {
            public string type;
            public string size;
            public int orbit;
            public float radius;
            public bool catalogName;
            public bool isHome;
            public ScenarioMoon[] moons;
        }

        [Serializable]
        private class ScenarioMoon
        {
            public string type;
            public string size;
            public int orbit;
            public bool catalogName;
            public bool isHome;
        }

        [Serializable]
        private class ScenarioStation
        {
            public string type;
            public int orbit;
        }

        private static Station[] CreateStations(ScenarioStation[] stationsDto, Fraction owner, StarSys sys)
        {
            if (stationsDto == null || stationsDto.Length == 0) return Array.Empty<Station>();

            var rng = new System.Random();
            var list = new List<Station>(stationsDto.Length);
            for (int i = 0; i < stationsDto.Length; i++)
            {
                var dto = stationsDto[i];
                var def = DefaultStationDef();
                var position = OrbitMath.PlanetOrbitIndexToUnits(dto.orbit);
                var station = StationCreator.Create(def, owner, new UnityEngine.Vector3(position, 0f, 0f));
                StationTradeBootstrap.InitForStation(ref station, rng);
                list.Add(station);
            }

            return list.ToArray();
        }

        private static StationTypeDef DefaultStationDef()
        {
            return new StationTypeDef
            {
                Key = "station_default",
                PrefabKey = "station_default",
                DefaultModules = new[] { EStationModuleType.Storage, EStationModuleType.Dock, EStationModuleType.Trade },
                BaseHull = 100f,
                BasePower = 50f
            };
        }
    }
}
