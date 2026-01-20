using System;
using System.Collections.Generic;
using _Project.DataAccess;
using _Project.Scripts.Galaxy.Data;

namespace _Project.Scripts.Galaxy.Generation
{
    /// <summary>Создаёт месторождения ресурсов для планет и лун после заселения галактики.</summary>
    public static class ResourceDepositsCreator
    {
        private static readonly Random Rng = new Random();
        private static bool _catalogReady;
        private static IReadOnlyDictionary<string, int> _idByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static IReadOnlyList<int> _mineableIds = Array.Empty<int>();
        private static readonly float PlanetSourceK = 1.0f;
        private static readonly float MoonSourceK = 0.75f;
        private static float starMetallicity = 0f;
        private static float GetFullness(StarSys starSys, int objAmmount)
        {
            float typeK = starSys.Star.type switch
            {
                EStarType.Black => 0.7f,
                EStarType.Neutron => 0.8f,
                EStarType.Blue => 0.6f,
                EStarType.White => 0.7f,
                EStarType.Orange => 0.4f,
                EStarType.Yellow => 0.5f,
                EStarType.Red => 0.3f,
                _ => 0.5f
            };

            float sizeK = starSys.Star.size switch
            {
                EStarSize.Dwarf => 1.0f,
                EStarSize.Normal => 0.7f,
                EStarSize.Giant => 0.4f,
                EStarSize.Supergiant => 0.2f,
                _ => 0.5f
            };
            float density = objAmmount <= 0 ? 0f : MathF.Min(1f, objAmmount / 12f);
            float objectsK = 0.4f + 0.6f * density; // 0.4 .. 1.0
            return typeK * sizeK * objectsK;
        }
        public static void AssignDeposits(StarSys[] galaxy)
        {
 
            for (int i = 0; i < galaxy.Length; i++)
            {
                starMetallicity = galaxy[i].Star.metallicity;
                MakeStarSys(galaxy[i]);
            }           
        }

        public static void MakeStarSys(StarSys starSys) // обработка звездной системы
        {
            int objectAmount = GetObjectsAmount(starSys);
            float sysFullness = GetFullness(starSys, objectAmount);
            for (int i = 0; i < starSys.PlanetSysArr.Length; i++)
            {
                MakePlanetSys(starSys.PlanetSysArr[i], sysFullness); // обработка планетарной системы
            }
        }

        public static void MakePlanetSys(PlanetSys planetSys, float fullness)
        {
            var planet = planetSys.Planet;
            planet.ResourceDeposits = GetPlanetDeposit(planet, fullness); // обработка планеты
            for (int i = 0; i < planetSys.Moons.Length; i++)
            {
                var moon =  planetSys.Moons[i];
                moon.ResourceDeposits = GetMoonDeposit(moon, planet, fullness); // обработка луны
            }
        }

        private static ResourceDeposit[] GetPlanetDeposit(Planet planet, float fullness)
        {
            EnsureCatalog();
            var candidates = new List<int>();
            void Add(string name)
            {
                if (_idByName.TryGetValue(name, out var id))
                    candidates.Add(id);
            }

            switch (planet.Type)
            {
                case EPlanetType.GasGiant:
                    Add("Noble Gases");
                    Add("Atmospheric Gases");
                    Add("Reactive Gases");
                    Add("Hydrocarbons");
                    break;
                case EPlanetType.IceGiant:
                    Add("Ice");
                    Add("Noble Gases");
                    Add("Atmospheric Gases");
                    Add("Hydrocarbons");
                    Add("Inorganic Carbon");
                    Add("Minerals");
                    break;
                case EPlanetType.Frozen:
                    Add("Ice");
                    Add("Noble Gases");
                    Add("Atmospheric Gases");
                    Add("Inorganic Carbon");
                    Add("Minerals");
                    Add("Salts");
                    break;
                case EPlanetType.Ocean:
                    Add("Salts");
                    Add("Minerals");
                    Add("Inorganic Carbon");
                    Add("Ice");
                    break;
                case EPlanetType.Desert:
                case EPlanetType.Stone:
                    Add("Structural Metals");
                    Add("Reactive Metals");
                    Add("Minerals");
                    Add("Salts");
                    Add("Heavy Metals");
                    Add("Precious Metals");
                    break;
                case EPlanetType.Lava:
                    Add("High-Tech Metals");
                    Add("Reactive Metals");
                    Add("Heavy Metals");
                    Add("Precious Metals");
                    Add("Radioactive Elements");
                    break;
                case EPlanetType.Toxic:
                    Add("Reactive Gases");
                    Add("Reactive Metals");
                    Add("Radioactive Elements");
                    Add("Heavy Metals");
                    break;
                case EPlanetType.Blasted:
                    Add("Radioactive Elements");
                    Add("Heavy Metals");
                    Add("Precious Metals");
                    break;
                default:
                    Add("Structural Metals");
                    Add("Reactive Metals");
                    Add("Minerals");
                    Add("Salts");
                    break;
            }
            if (candidates.Count == 0)
                candidates.AddRange(_mineableIds);
            return GenerateDeposits(candidates, fullness, PlanetSourceK);
        }

        private static ResourceDeposit[] GetMoonDeposit(Moon moon, Planet planet, float fullness)
        {
            EnsureCatalog();
            var candidates = new List<int>();
            void Add(string name)
            {
                if (_idByName.TryGetValue(name, out var id))
                    candidates.Add(id);
            }

            switch (moon.Type)
            {
                case EMoonType.Ice:
                    Add("Ice");
                    Add("Noble Gases");
                    Add("Atmospheric Gases");
                    Add("Inorganic Carbon");
                    Add("Salts");
                    break;
                case EMoonType.Ocean:
                    Add("Salts");
                    Add("Minerals");
                    Add("Inorganic Carbon");
                    Add("Ice");
                    break;
                case EMoonType.Desert:
                case EMoonType.Stone:
                    Add("Structural Metals");
                    Add("Reactive Metals");
                    Add("Minerals");
                    Add("Salts");
                    break;
                case EMoonType.Lava:
                    Add("High-Tech Metals");
                    Add("Reactive Metals");
                    Add("Heavy Metals");
                    Add("Precious Metals");
                    break;
                case EMoonType.Toxic:
                    Add("Reactive Gases");
                    Add("Reactive Metals");
                    Add("Radioactive Elements");
                    break;
                case EMoonType.Blasted:
                    Add("Radioactive Elements");
                    Add("Heavy Metals");
                    break;
                default:
                    Add("Minerals");
                    Add("Structural Metals");
                    Add("Reactive Metals");
                    break;
            }

            if (candidates.Count == 0)
                candidates.AddRange(_mineableIds);

            return GenerateDeposits(candidates, fullness, MoonSourceK);
        }

        private static int GetObjectsAmount(StarSys starSys)
        {
            var planets = starSys.PlanetSysArr;
            if (planets == null || planets.Length == 0) return 0;
            int count = planets.Length;
            for (int i = 0; i < planets.Length; i++)
            {
                var moons = planets[i].Moons;
                if (moons != null)
                    count += moons.Length;
            }
            return count;
        }

        private static void EnsureCatalog()
        {
            if (_catalogReady)
                return;

            CATALOG.LoadAll();
            var skuList = CATALOG.Sku;
            var ids = new List<int>();
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < skuList.Count; i++)
            {
                var sku = skuList[i];
                if (!map.ContainsKey(sku.Name))
                    map[sku.Name] = sku.Id;
                if (sku.IsMineable && !sku.IsLootOnly)
                    ids.Add(sku.Id);
            }

            _mineableIds = ids;
            _idByName = map;
            _catalogReady = true;
        }

        private static ResourceDeposit[] GenerateDeposits(IReadOnlyList<int> candidates, float fullness, float k)
        {
            if (candidates == null || candidates.Count == 0)
                return Array.Empty<ResourceDeposit>();

            int count = Math.Clamp(1 + (int)(fullness * 2f) + Rng.Next(0, 2), 1, 5);
            var deposits = new ResourceDeposit[count];
            for (int i = 0; i < count; i++)
            {
                int resId = candidates[Rng.Next(candidates.Count)];
                float level = fullness * (0.6f + 0.4f * (float)Rng.NextDouble());
                level = Math.Clamp(level, 0.1f, 1f);
                float availability = fullness * k * starMetallicity * (0.8f + 0.2f * (float)Rng.NextDouble());
                availability = Math.Clamp(availability, 0.0f, 1.0f);
                deposits[i] = new ResourceDeposit
                {
                    DepositId = i + 1,
                    ResourceId = resId,
                    ResourcePurity = level,
                    Availability = availability
                };
            }

            return deposits;
        }

    }
}
