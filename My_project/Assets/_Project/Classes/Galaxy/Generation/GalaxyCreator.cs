using System;
using _Project.Scripts.Galaxy.Data;
using UnityEngine;

namespace _Project.Scripts.Galaxy.Generation
{
    public static class GalaxyCreator
    {
        // === Параметры генерации ===
        private const int   StarCount                 = 1300;  // Количество звезд в галактике
        private const float GalaxyRadius              = 500f;  // Радиус галактики (условные единицы)
        private const float GalaxyStarLayer           = 0f;    // Слой расположения звезд по оси Z
        private const float DensityArms               = 3f;    // Коэффициент плотности спиральных рукавов
        private const float WidthArms                 = 60f;   // Ширина спиральных рукавов
        private const float MinStarInterval           = 3.5f;  // Минимальная дистанция между звездами
        private const float CentralBlackHoleIntervalK = 10f;   // Множитель дистанции рядом с ядром
        private const int   MaxAttemptsPerStar        = 64;    // Максимум попыток подобрать позицию

        // Служебные значения
        private static float _lastRawX;
        private static float _lastRawY;

        public static StarSys[] Create()
        {
            var galaxy = CreateSpiralGalaxy(StarCount, GalaxyStarLayer); // Создаем заготовку спиральной галактики
            LocalizationDatabase.PrepareStarNames(galaxy.Length);
            LocalizationDatabase.ResetDynamicValues();

            for (int i = 0; i < galaxy.Length; i++) // Обрабатываем каждую систему по порядку
            {
                ref var sysData = ref galaxy[i];
                sysData.NameId = i;

                if (i == 0)
                {
                    // В центре оставляем исходную систему
                    //
                    // Возможный вариант ядра: черная дыра без планет
                    //var starBh = new Star { type = EStarType.Black, size = EStarSize.Supergiant }; // size пока не используется
                    //int[] noPlanets = Array.Empty<int>();
                    //galaxy[i] = StarSysCreator.Create(galaxy[i], starBh, noPlanets);
                    var coreStar = sysData.Star;
                    coreStar.NameId = i;
                    coreStar.OldX = sysData.OldX;
                    coreStar.OldY = sysData.OldY;
                    sysData.Star = coreStar;
                    continue;
                }

                var star = StarCreator.Create(); // Создаем звезду
                star.NameId = i;
                star.OldX = sysData.OldX;
                star.OldY = sysData.OldY;

                var planetOrbits = PlanetOrbitCreator.Create(star); // Строим орбиты планет вокруг звезды
                var planetsArr = new Planet[planetOrbits.Length];   // Массив планет для текущей системы
                var planetSysArr = new PlanetSys[planetOrbits.Length]; // Массив систем планеты со спутниками
                var starDisplayName = LocalizationDatabase.GetStarName(star.NameId, star.OldX, star.OldY);

                for (var j = 0; j < planetOrbits.Length; j++) // Создаем планеты и их спутники
                {
                    var planet = PlanetCreator.Create(planetOrbits[j], star); // Создаем планету для выбранной орбиты
                    var moonOrbits = MoonOrbitCreator.Create(planet);     // Строим орбиты спутников планеты
                    var moonsArr = new Moon[moonOrbits.Length];                  // Заготовка под спутники

                    for (var k = 0; k < moonOrbits.Length; k++) // Генерируем спутники планеты
                    {
                        var moon = MoonCreator.Create(star, planetOrbits[j], planet, moonOrbits[k]); // Создаем спутник
                        if (!string.IsNullOrWhiteSpace(starDisplayName))
                        {
                            var moonName = LocalizationDatabase.ComposeMoonName(starDisplayName, j, k);
                            var moonId = LocalizationDatabase.RegisterDynamicValue(moonName);
                            if (moonId != int.MinValue)
                                moon.NameId = moonId;
                        }
                        moonsArr[k] = moon;
                    }

                    if (!string.IsNullOrWhiteSpace(starDisplayName))
                    {
                        var planetName = LocalizationDatabase.ComposePlanetName(starDisplayName, j);
                        var planetId = LocalizationDatabase.RegisterDynamicValue(planetName);
                        if (planetId != int.MinValue)
                            planet.NameId = planetId;
                    }

                    planetsArr[j] = planet;

                    planetSysArr[j] = PlanetSysCreator.Create(star, planetOrbits[j], planet, moonOrbits, moonsArr); // Собираем систему планеты и ее спутников
                }

                sysData = StarSysCreator.Create(sysData, star, planetSysArr, planetOrbits); // Обновляем звездную систему с учетом планет
            }

            return galaxy;
        }

        private static StarSys[] CreateSpiralGalaxy(int count, float zLayer)
        {
            if (count <= 0) return Array.Empty<StarSys>();
            var arr = new StarSys[count];
            // Ядро
            arr[0] = new StarSys // Центральная звездная система
            {
                GalaxyPosition = new Vector3(0f, 0f, zLayer),
                OldX = 0f,
                OldY = 0f
            };
            // Размещаем остальные системы
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

        private static Vector3 GenerateStarsNoGaussianDistr(float zLayer)
        {
            // Берем X из [-1;1], избегаем нулевого значения (иначе Atan(y/x))
            float xSeed = UnityEngine.Random.Range(-1f, 1f);
            if (Mathf.Approximately(xSeed, 0f)) xSeed = 0.0001f;

            float y = UnityEngine.Random.Range(-GalaxyRadius, GalaxyRadius);

            // Смещаем координату в зависимости от |xSeed|, чтобы избежать NaN и скученности точек
            float xCore = Mathf.Pow(Mathf.Abs(xSeed), DensityArms) * GalaxyRadius
                        + UnityEngine.Random.Range(-WidthArms, WidthArms);

            float sign = UnityEngine.Random.value > 0.5f ? 1f : -1f;
            float x = xCore * sign;

            _lastRawX = x;
            _lastRawY = y;

            return TwistCoordinates(new Vector3(x, y, zLayer));
        }

        // Идея: Atan(y/x) плюс плавное растягивание для формирования рукавов
        private static Vector3 TwistCoordinates(Vector3 vec)
        {
            // Защищаемся от деления на ноль при проекции на ось X
            float xSafe = Mathf.Abs(vec.x) < 1e-4f ? (vec.x >= 0f ? 1e-4f : -1e-4f) : vec.x;

            float angle  = Mathf.Atan(vec.y / xSafe); // Угол полярных координат
            float radius = Mathf.Sqrt(vec.x * vec.x + vec.y * vec.y);

            // Определяем, в какой рукав попадет точка
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

                // Пропускаем кандидатов с нечисловыми координатами
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
