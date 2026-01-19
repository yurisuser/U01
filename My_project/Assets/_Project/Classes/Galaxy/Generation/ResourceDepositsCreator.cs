using System;
using System.Collections.Generic;
using _Project.DataAccess;
using _Project.Scripts.Galaxy.Data;

namespace _Project.Scripts.Galaxy.Generation
{
    /// <summary>Создаёт месторождения ресурсов для планет и лун после заселения галактики.</summary>
    public static class ResourceDepositsCreator
    {
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
        private static float starMetallicity = 0f;
        public static void AssignDeposits(StarSys[] galaxy)
        {
 
            for (int i = 0; i < galaxy.Length; i++)
            {
                MakeStarSys(galaxy[i]);
                starMetallicity = galaxy[i].Star.metallicity;
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
            return new ResourceDeposit[0];
        }

        private static ResourceDeposit[] GetMoonDeposit(Moon moon, Planet planet, float fullness)
        {
            return new ResourceDeposit[0];
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


    }
}
