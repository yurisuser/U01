using System;
using UnityEngine;
using static _Project.CONSTANT.GALAXY; // твои константы: PlanetRockyWeight и т.д.

namespace _Project.Scripts.Galaxy.Data
{
    public static class PlanetCreator
    {
        public static Planet Create(int orbitIndex)
        {
            return new Planet
            {
                Name             = "name",
                Mass             = 5,
                Type             = PlanetType.GasGiant,
                Atmosphere       = 1,
                Radius           = 2,
                OrbitalDistance  = 2,
                OrbitalPeriod    = 2,
                Temperature      = 2,
                Gravity          = 2,
            };
        }

        // ----------------- helpers -----------------

        // Выбор типа планеты: базовые веса из констант + лёгкий сдвиг по орбите
    }
}
