using System;
using UnityEngine;
using static _Project.CONSTANT.GALAXY; // твои константы: PlanetRockyWeight и т.д.

namespace _Project.Scripts.Galaxy.Data
{
    public static class PlanetCreator
    {
        /// <summary>
        /// Простой генератор планеты по номеру орбиты (1.. N).
        /// Детерминизм обеспечивай снаружи через UnityEngine. Random. InitState(...).
        /// Никаких реальных радиусов орбит: расстояние = orbitIndex, период ~ distance^1.5.
        /// </summary>
        public static Planet Create(int orbitIndex)
        {
            int orb = Mathf.Max(1, orbitIndex);

            // Тип
            string type = PickType(orb);

            // Размеры/массы по типу (берём твои профили)
            (float mMin, float mMax, float rMin, float rMax) range = type switch
            {
                "gas_giant" => PlanetHuge,
                "ice_giant" => PlanetLarge,
                "dwarf"     => PlanetSmall,
                _           => (UnityEngine.Random.value < 0.5f ? PlanetSmall : PlanetMedium)
            };

            float mass   = Rand(range.mMin, range.mMax);
            float radius = Rand(range.rMin, range.rMax);

            // Атмосфера (просто профиль по типу)
            float atmosphere = type switch
            {
                "gas_giant" => Rand(1.5f, 3.0f),
                "ice_giant" => Rand(0.6f, 1.4f),
                "ocean"     => Rand(0.7f, 1.4f),
                "lava"      => Rand(0.8f, 1.6f),
                "toxic"     => Rand(1.0f, 2.0f),
                "frozen"    => Rand(0.1f, 0.6f),
                _           => Rand(0.2f, 1.3f) // rocky/desert/dwarf
            };

            // «Орбитальные» величины (абстрактные ед.)
            float orbitalDistance = orb;
            float orbitalPeriod   = Mathf.Pow(orbitalDistance, 1.5f);

            // Температура: грубый спад от расстояния + поправка по типу
            float tBase        = 420f / Mathf.Sqrt(orbitalDistance);
            float typeTFactor  = TypeTempFactor(type);
            float temperature  = Mathf.Clamp(tBase * typeTFactor + Rand(-20f, 20f), 30f, 800f);

            // Гравитация (условная): m / r^2
            float gravity = Mathf.Clamp(mass / Mathf.Max(0.1f, radius * radius), 0.05f, 5f);

            // Имя
            string name = $"P{orb}-{UnityEngine.Random.Range(100, 999)}";

            // Ресурсы — пока пусто (заполнишь позже своей логикой)
            var resources = Array.Empty<PlanetResource>();

            return new Planet
            {
                Name             = name,
                Mass             = mass,
                Type             = type,
                Atmosphere       = atmosphere,
                Radius           = radius,
                OrbitalDistance  = orbitalDistance,
                OrbitalPeriod    = orbitalPeriod,
                Temperature      = temperature,
                Gravity          = gravity,
                Resources        = resources
            };
        }

        // ----------------- helpers -----------------

        // Выбор типа планеты: базовые веса из констант + лёгкий сдвиг по орбите
        private static string PickType(int orbit)
        {
            // ближе к звезде — чаще каменные/лава/океан; дальше — газ/лёд/замёрзшие
            float fGas   = (orbit >= 9)  ? 2.0f : 0.3f;
            float fIce   = (orbit >= 10) ? 1.8f : 0.3f;
            float fRock  = (orbit <= 8)  ? 1.4f : 0.7f;
            float fLava  = (orbit <= 6)  ? 1.5f : 0.4f;
            float fOcean = (orbit <= 10) ? 1.2f : 0.8f;
            float fDes   = (orbit <= 10) ? 1.2f : 1.0f;
            float fFrzn  = (orbit >= 12) ? 1.5f : 0.7f;
            float fDwarf = (orbit <= 10) ? 1.0f : 0.8f;
            float fToxic = 1.0f;

            int wRocky   = Scale(PlanetRockyWeight,   fRock);
            int wGas     = Scale(PlanetGasGiantWeight, fGas);
            int wIce     = Scale(PlanetIceGiantWeight, fIce);
            int wDwarf   = Scale(PlanetDwarfWeight,    fDwarf);
            int wOcean   = Scale(PlanetOceanWeight,    fOcean);
            int wDesert  = Scale(PlanetDesertWeight,   fDes);
            int wLava    = Scale(PlanetLavaWeight,     fLava);
            int wFrozen  = Scale(PlanetFrozenWeight,   fFrzn);
            int wToxic   = Scale(PlanetToxicWeight,    fToxic);

            // рулетка
            int sum =
                wRocky + wGas + wIce + wDwarf + wOcean +
                wDesert + wLava + wFrozen + wToxic;

            if (sum <= 0) return "rocky"; // fallback

            int roll = UnityEngine.Random.Range(0, sum);
            int acc  = 0;

            if ((acc += wRocky)  > roll) return "rocky";
            if ((acc += wGas)    > roll) return "gas_giant";
            if ((acc += wIce)    > roll) return "ice_giant";
            if ((acc += wDwarf)  > roll) return "dwarf";
            if ((acc += wOcean)  > roll) return "ocean";
            if ((acc += wDesert) > roll) return "desert";
            if ((acc += wLava)   > roll) return "lava";
            if ((acc += wFrozen) > roll) return "frozen";
            return "toxic";
        }

        private static int Scale(int baseWeight, float factor)
            => Mathf.Max(0, Mathf.RoundToInt(baseWeight * factor));

        private static float Rand(float min, float max)
            => UnityEngine.Random.Range(min, max);

        private static float TypeTempFactor(string type) => type switch
        {
            "gas_giant" => 0.8f,   // массивные атмосферы → равномернее
            "ice_giant" => 0.7f,
            "ocean"     => 0.9f,
            "lava"      => 1.2f,
            "frozen"    => 0.6f,
            "toxic"     => 1.05f,
            _           => 1.0f    // rocky, desert, dwarf
        };
    }
}
