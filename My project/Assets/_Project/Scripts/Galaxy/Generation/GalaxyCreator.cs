using System;
using _Project.Scripts.Galaxy.Data;
using UnityEngine;

namespace _Project.Scripts.Galaxy.Generation
{
    public static class GalaxyCreator
    {
        // === настройки ===
        private const int   StarCount                 = 1300;
        private const float GalaxyRadius              = 100f;
        private const float GalaxyStarLayer           = 0f;
        private const float DensityArms               = 4.2f;
        private const float WidthArms                 = 10f;
        private const float MinStarInterval           = 1.5f;
        private const float CentralBlackHoleIntervalK = 7f;
        private const int   MaxAttemptsPerStar        = 64;
        const int GlobalSeed = 1337;

        // внутреннее
        private static float _lastRawX;
        private static float _lastRawY;
        
public static StarSys[] Create()
{
    StarSys[] galaxy = CreateSpiralGalaxy(StarCount, GalaxyStarLayer); // создаём шаблон

    for (int si = 0; si < galaxy.Length; si++)
    {
        // сид для системы — все последующие генераторы в системе будут детерминированы
        UnityEngine.Random.InitState(Seed(GlobalSeed, si, 0, 0));
        if (si == 0)
        {
            // Центральный объект: чёрная дыра, без планет
            var starBh = new Star { type = StarType.Black, size = StarSize.Supergiant }; // size по желанию
            int[] noPlanets = Array.Empty<int>();
            galaxy[si] = StarSysCreator.Create(galaxy[si], starBh, noPlanets);
            continue;
        }

        // 1) Звезда
        Star star = StarCreator.Create();
        PlanetSysCreator.ResetPerSystem();

        // 2) Планеты: только номера орбит (int[]), без радиусов
        int[] planetOrbits = PlanetOrbitCreator.Create(star);

        // 3) Для каждой планеты — свой сид на основе (seed, systemIndex, planetOrbit)
        for (int pj = 0; pj < planetOrbits.Length; pj++)
        {
            int planetOrbit = planetOrbits[pj];

            UnityEngine.Random.InitState(Seed(GlobalSeed, si, planetOrbit, 0));
            Planet planet = PlanetCreator.Create(planetOrbit, star);

            // 4) Луны: только номера орбит
            int[] moonOrbits = MoonOrbitCreator.Create(planet);

            // 5) Для каждой луны — сид на основе (seed, systemIndex, planetOrbit, moonOrbit)
            for (int mk = 0; mk < moonOrbits.Length; mk++)
            {
                int moonOrbit = moonOrbits[mk];

                UnityEngine.Random.InitState(Seed(GlobalSeed, si, planetOrbit, moonOrbit));
                Moon moon = MoonCreator.Create(star, planetOrbit, planet, moonOrbit);

                // Оставляю как у тебя: PlanetSysCreator вызывается на каждой луне
                PlanetSys planetSys = PlanetSysCreator.Create(star, planetOrbit, planet, moon);
            }
        }

        // 6) Собираем звёздную систему и кладём её в массив (фикс: присваиваем в galaxy[si])
        StarSys starSys = StarSysCreator.Create(galaxy[si], star, planetOrbits);
        galaxy[si] = starSys;
    }

    return galaxy;

    // Локальная детерминированная мешалка для сидов
    static int Seed(int globalSeed, int systemIndex, int planetOrbit, int moonOrbit)
    {
        unchecked
        {
            int h = globalSeed;
            h = h * 31 + systemIndex;
            h = h * 31 + planetOrbit;
            h = h * 31 + moonOrbit;
            return h;
        }
    }
}
        private static StarSys[] CreateSpiralGalaxy(int count, float zLayer)
        {
            if (count <= 0) return Array.Empty<StarSys>();
            var arr = new StarSys[count];
            // центр
            arr[0] = new StarSys //Центральная черная дыра
            {
                GalaxyPosition = new Vector3(0f, 0f, zLayer),
                OldX = 0f,
                OldY = 0f
            };
            //Другие системы
            for (int i = 1; i < count; i++)
            {
                var sys = new StarSys();

                Vector3 pos = PlaceWithMinDistance(
                    index: i,
                    placed: arr,
                    sampleFunc: () => GenerateStarsNoGaussianDistr(zLayer),
                    baseMinDist: MinStarInterval,
                    centerExtraK: CentralBlackHoleIntervalK,
                    maxAttempts: MaxAttemptsPerStar
                );

                sys.GalaxyPosition = pos;
                sys.OldX = _lastRawX;
                sys.OldY = _lastRawY;

                arr[i] = sys;
            }

            return arr;
        }
        // — старое распределение, но без NaN от Pow —
        private static Vector3 GenerateStarsNoGaussianDistr(float zLayer)
        {
            // семя X в [-1;1], избегаем точного нуля (для Atan(y/x))
            float xSeed = UnityEngine.Random.Range(-1f, 1f);
            if (Mathf.Approximately(xSeed, 0f)) xSeed = 0.0001f;

            float y = UnityEngine.Random.Range(-GalaxyRadius, GalaxyRadius);

            // ВНИМАНИЕ: степень берём от |xSeed|, знак задаём позже — так не будет NaN при нецелой степени
            float xCore = Mathf.Pow(Mathf.Abs(xSeed), DensityArms) * GalaxyRadius
                        + UnityEngine.Random.Range(-WidthArms, WidthArms);

            float sign = UnityEngine.Random.value > 0.5f ? 1f : -1f;
            float x = xCore * sign;

            _lastRawX = x;
            _lastRawY = y;

            return TwistCoordinates(new Vector3(x, y, zLayer));
        }

        // — как было: Atan(y/x) + смена знака радиуса для второго рукава —
        private static Vector3 TwistCoordinates(Vector3 vec)
        {
            // защищаемся от деления на ноль маленьким эпсилоном со знаком X
            float xSafe = Mathf.Abs(vec.x) < 1e-4f ? (vec.x >= 0f ? 1e-4f : -1e-4f) : vec.x;

            float angle  = Mathf.Atan(vec.y / xSafe); // как в старом файле
            float radius = Mathf.Sqrt(vec.x * vec.x + vec.y * vec.y);

            // этот приём и даёт второй рукав
            radius *= (vec.x - vec.y) < 0f ? -1f : 1f;

            float normalizedRadius = Mathf.Abs(radius) / GalaxyRadius;
            float newAngle = angle + normalizedRadius * normalizedRadius * 4f;

            float x = Mathf.Cos(newAngle) * radius;
            float y = Mathf.Sin(newAngle) * radius;

            return new Vector3(x, y, GalaxyStarLayer);
        }

        private static Vector3 PlaceWithMinDistance(int index, StarSys[] placed, Func<Vector3> sampleFunc, float baseMinDist, float centerExtraK, int maxAttempts)
        {
            Vector3 lastSample = default;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var candidate = sampleFunc();
                lastSample = candidate;

                // фильтр на случай каких-то экзотических значений
                if (!IsFinite(candidate.x) || !IsFinite(candidate.y)) continue;

                bool ok = true;
                for (int j = 0; j < index; j++)
                {
                    float k = (j == 0) ? centerExtraK : 1f;
                    float minDist = baseMinDist * k;

                    if (Vector3.Distance(candidate, placed[j].GalaxyPosition) < minDist)
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok) return candidate;
            }

            return lastSample;
        }

        private static bool IsFinite(float v) => !(float.IsNaN(v) || float.IsInfinity(v));
    }
}
