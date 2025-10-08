using System;
using Unity.VisualScripting;
using UnityEngine;
using Random = System.Random;

namespace _Project.Scripts.Galaxy.Data
{
    public static class PlanetCreator
    {
        private static readonly PlanetType[] PlanetTypeValues = (PlanetType[])Enum.GetValues(typeof(PlanetType));
        public static Planet Create(int orbitIndex, Star star)
        {
            //Debug.Log($"Create planet {PlanetTypeValues.Length} of {star.name}");
            return new Planet
            {
                Name             = $"/{star.name} {orbitIndex}",
                Mass             = 5,
                Type             = GetPlanetType(star.type, orbitIndex),
                Atmosphere       = 1,
                Radius           = 2,
                OrbitalDistance  = 2,
                OrbitalPeriod    = 2,
                Temperature      = 2,
                Gravity          = 2,
            };
        }
        
        private static PlanetType GetPlanetType(StarType starType, int orbit)
        {
            return PlanetType.GasGiant;
            int a = UnityEngine.Random.Range(0, 8);
            return PlanetTypeValues[a];
        }

    }
}
