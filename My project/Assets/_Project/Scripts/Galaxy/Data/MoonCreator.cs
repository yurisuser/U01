using UnityEngine;
using static _Project.CONSTANT.GALAXY; // Moon*Weight, OrbitSlots, Moon* profiles

namespace _Project.Scripts.Galaxy.Data
{
    public static class MoonCreator
    {
        /// <summary>
        /// Простой генератор луны.
        /// Держимся твоей модели: только номера орбит, круговые орбиты, детерминизм задаёшь Random. InitState(...) снаружи.
        /// </summary>
        public static Moon Create(Star star, int planetOrbit, Planet planet, int moonOrbit)
        {
            // орбитальные «абстрактные» величины
            int   orbitIndex     = Mathf.Clamp(moonOrbit, 1, Mathf.Max(1, OrbitSlots));
            float orbitDistance  = orbitIndex;                // без радиусов — просто индекс
            float orbitPeriod    = Mathf.Pow(orbitDistance, 1.4f) * 0.3f; // чуть быстрее, чем у планет

            // тип и размер
            var type = PickType(planet, orbitIndex);
            var size = PickSize(planet, type);

            // масса/радиус по профилям
            var (mMin, mMax, rMin, rMax) = size switch
            {
                MoonSize.Tiny   => MoonTiny,
                MoonSize.Small  => MoonSmall,
                MoonSize.Medium => MoonMedium,
                MoonSize.Large  => MoonLarge,
                _               => MoonSmall
            };
            float mass   = Rand(mMin, mMax);
            float radius = Rand(rMin, rMax);

            // атмосфера (луны в основном «тонкие»; исключения — крупные и «океанические»)
            float atmosphere = type switch
            {
                MoonType.Ocean     => Rand(0.2f, size == MoonSize.Large ? 1.0f : 0.6f),
                MoonType.Volcanic  => Rand(0.0f, 0.3f),
                MoonType.Icy       => Rand(0.0f, 0.15f),
                MoonType.Captured  => Rand(0.0f, 0.05f),
                MoonType.Desert    => Rand(0.0f, 0.2f),
                _ /*Rocky*/        => Rand(0.0f, size == MoonSize.Large ? 0.4f : 0.2f)
            };

            // наклон: «захваченные» чаще наклонные
            float inclination = (type == MoonType.Captured)
                ? Rand(10f, 40f)
                : Rand(0f, 12f);

            // температура: от температуры планеты с поправками по типу
            float baseTemp = Mathf.Clamp(planet.Temperature + Rand(-35f, 25f), 30f, 800f);
            baseTemp *= type switch
            {
                MoonType.Volcanic => 1.20f,
                MoonType.Icy      => 0.75f,
                MoonType.Ocean    => 0.95f,
                _                 => 1.0f
            };
            float temperature = Mathf.Clamp(baseTemp, 30f, 800f);

            // «поверхностная» гравитация ~ m / r^2
            float gravity = Mathf.Clamp(mass / Mathf.Max(0.05f, radius * radius), 0.01f, 0.7f);

            // имя
            string name = $"M{planetOrbit}.{orbitIndex}-{Random.Range(100, 999)}";

            return new Moon
            {
                Name           = name,
                Type           = type,
                Size           = size,
                OrbitIndex     = orbitIndex,
                Mass           = mass,
                Radius         = radius,
                OrbitDistance  = orbitDistance,
                OrbitPeriod    = orbitPeriod,
                Inclination    = inclination,
                Atmosphere     = atmosphere,
                Temperature    = temperature,
                Gravity        = gravity
            };
        }

        // ---------------- helpers ----------------

        private static MoonType PickType(Planet planet, int moonOrbit)
        {
            // базовые веса из констант
            int wRocky   = MoonRockyWeight;
            int wIcy     = MoonIcyWeight;
            int wVolc    = MoonVolcanicWeight;
            int wDesert  = MoonDesertWeight;
            int wOcean   = MoonOceanWeight;
            int wCaptured= MoonCapturedWeight;

            // поправки по типу планеты
            string pt = (planet.Type ?? "rocky").ToLowerInvariant();
            switch (pt)
            {
                case "gas_giant":
                    wIcy      = Scale(wIcy,      1.8f);
                    wCaptured = Scale(wCaptured, 2.0f);
                    wRocky    = Scale(wRocky,    1.2f);
                    wOcean    = Scale(wOcean,    0.6f);
                    break;
                case "ice_giant":
                    wIcy      = Scale(wIcy,      1.7f);
                    wCaptured = Scale(wCaptured, 1.6f);
                    break;
                case "rocky":
                    wRocky    = Scale(wRocky,    1.5f);
                    wVolc     = Scale(wVolc,     (moonOrbit <= 3) ? 1.8f : 1.2f);
                    break;
                case "ocean":
                    wOcean    = Scale(wOcean,    1.6f);
                    wIcy      = Scale(wIcy,      1.2f);
                    break;
                case "lava":
                    wVolc     = Scale(wVolc,     2.0f);
                    wIcy      = Scale(wIcy,      0.6f);
                    break;
                case "frozen":
                    wIcy      = Scale(wIcy,      2.0f);
                    wOcean    = Scale(wOcean,    0.5f);
                    break;
                case "desert":
                    wDesert   = Scale(wDesert,   1.6f);
                    break;
                case "toxic":
                    wCaptured = Scale(wCaptured, 1.4f);
                    break;
                case "dwarf":
                    wCaptured = Scale(wCaptured, 1.8f);
                    wRocky    = Scale(wRocky,    1.2f);
                    wOcean    = Scale(wOcean,    0.5f);
                    break;
            }

            // рулетка
            int sum = wRocky + wIcy + wVolc + wDesert + wOcean + wCaptured;
            if (sum <= 0) return MoonType.Rocky;

            int roll = Random.Range(0, sum);
            int acc  = 0;

            if ((acc += wRocky)    > roll) return MoonType.Rocky;
            if ((acc += wIcy)      > roll) return MoonType.Icy;
            if ((acc += wVolc)     > roll) return MoonType.Volcanic;
            if ((acc += wDesert)   > roll) return MoonType.Desert;
            if ((acc += wOcean)    > roll) return MoonType.Ocean;
            return MoonType.Captured;
        }

        private static MoonSize PickSize(Planet planet, MoonType type)
        {
            bool isGiantPlanet = planet.Type is "gas_giant" or "ice_giant";
            // базовая шкала
            int wTiny, wSmall, wMedium, wLarge;
            if (isGiantPlanet)
            {
                wTiny = 10; wSmall = 30; wMedium = 35; wLarge = 25;
            }
            else
            {
                wTiny = 40; wSmall = 45; wMedium = 12; wLarge = 3;
            }

            // небольшие поправки по типу луны
            if (type == MoonType.Volcanic) { wTiny += 5; wSmall += 5; }
            if (type == MoonType.Ocean)    { wSmall += 5; wMedium += 5; }
            if (type == MoonType.Captured) { wLarge = Mathf.Max(0, wLarge - 10); }

            int sum = wTiny + wSmall + wMedium + wLarge;
            int roll = Random.Range(0, sum);
            int acc = 0;

            if ((acc += wTiny)   > roll) return MoonSize.Tiny;
            if ((acc += wSmall)  > roll) return MoonSize.Small;
            if ((acc += wMedium) > roll) return MoonSize.Medium;
            return MoonSize.Large;
        }

        private static int   Scale(int w, float f)   => Mathf.Max(0, Mathf.RoundToInt(w * f));
        private static float Rand(float a, float b)  => Random.Range(a, b);
    }
}
