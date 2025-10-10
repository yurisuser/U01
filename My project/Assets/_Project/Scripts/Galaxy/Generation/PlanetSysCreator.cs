using System;
using System.Collections.Generic;
using _Project.Scripts.Galaxy.Data;
using UnityEngine;

namespace _Project.Scripts.Galaxy.Generation
{
    public static class PlanetSysCreator
    {
        // ===================== ТЮНИНГ =====================
        // Перевод «индекса орбиты» планеты в сценовые юниты.
        // Если у тебя уже есть своя функция масштаба — просто подставь её.
        private const float OrbitUnitPlanet = 10f; // юнитов сцены на 1 «клетку» орбиты планеты

        // Перевод «индекса орбиты» луны в сценовые юниты относительно ПЛАНЕТЫ.
        private const float OrbitUnitMoon   = 1.6f; // радиус внешней лунной орбиты = lastMoonIndex * этот множитель

        // Перевод радиуса планеты (в земных радиусах) в сценовые юниты её визуала.
        private const float PlanetRadiusToUnits = 0.6f;

        // Базовая минимальная угловая дистанция (радианы) «против прилипания»,
        // добавляется к расчётной ширине от габаритов систем.
        private const float MinAngularGapBase = 0.10f; // ≈ 5.7°

        // Шаг подбора угла (золотой угол, чтобы не «резонировать» с кратными делениями круга)
        private const float GoldenAngleRad = 2.39996322972865332f; // 360° * (φ - 1)^2

        // Сколько попыток смещения угла делаем, если рядом уже занято
        private const int MaxAngleAttempts = 64;

        // ==================================================

        // Для контроля соседства: накапливаем выставленные углы по каждой звезде.
        // Ключ — id звезды (Star.id); значение — список уже назначенных «угол+масштабы».
        private static readonly Dictionary<int, List<PlacedPlanet>> PlacedByStar = new();

        private struct PlacedPlanet
        {
            public int OrbitIndex;
            public float AngleRad;
            public float OrbitRadius; // радиус орбиты вокруг звезды (юниты сцены)
            public float Envelope;    // «внешний габарит» системы планеты (радиус визуала + дальнейшая лунная орбита), юниты
        }

        /// <summary>
        /// Собирает PlanetSys и назначает OrbitPosition (радианы).
        /// </summary>
        public static PlanetSys Create(Star star, int planetOrbitIndex, Planet planet, int[] moonOrbits, Moon[] moons)
        {
            // 1) Переводим «индекс орбиты планеты» в сценовый радиус
            float orbitRadius = OrbitIndexToUnits(planetOrbitIndex);

            // 2) Считаем внешний габарит планетной системы: визуальный радиус планеты + «самая дальняя» лунная орбита
            float planetVisual = Mathf.Max(0f, planet.Radius) * PlanetRadiusToUnits;
            int farMoonIndex = (moonOrbits != null && moonOrbits.Length > 0) ? moonOrbits[moonOrbits.Length - 1] : 0;
            float farMoonOrbit = farMoonIndex * OrbitUnitMoon;
            float envelope = planetVisual + farMoonOrbit;

            // 3) Подбираем угол с учётом уже расставленных планет у этой звезды
            float angle = PickAngleDeterministicAndSafe(star.id, planetOrbitIndex, orbitRadius, envelope);

            // 4) Регистрируем размещение (чтобы следующие планеты этой же звезды учитывали его)
            RegisterPlaced(star.id, planetOrbitIndex, angle, orbitRadius, envelope);

            // 5) Собираем PlanetSys
            return new PlanetSys
            {
                MotherStar  = star,
                OrbitIndex  = planetOrbitIndex,
                Planet      = planet,
                OrbitPosition = angle, // радианы — единый формат; если нужен градус, переведи при отрисовке
                Moons       = moons ?? Array.Empty<Moon>()
            };
        }

        // === вспомогательное ===

        private static float OrbitIndexToUnits(int orbitIndex)
        {
            // Можно усложнить (например, экспонентой), но линейный масштаб обычно достаточно удобен для карты.
            return Mathf.Max(0, orbitIndex) * OrbitUnitPlanet;
        }

        private static float PickAngleDeterministicAndSafe(int starId, int orbitIndex, float orbitR, float envelope)
        {
            // Базовый угол — детерминированный: хэш(id, orbit) → [0..2π)
            float baseAngle = Hash01(starId * 73856093 ^ orbitIndex * 19349663) * Mathf.PI * 2f;

            // Пытаемся ставить базовый угол; если конфликт — добавляем GOLDEN_ANGLE_RAD и пробуем снова.
            // Требуемая минимальная угловая дистанция от уже размещённых = базовая + вклад от габаритов систем.
            float angle = baseAngle;
            if (!PlacedByStar.TryGetValue(starId, out var placed) || placed.Count == 0)
                return angle;

            for (int attempt = 0; attempt < MaxAngleAttempts; attempt++, angle += GoldenAngleRad)
            {
                bool ok = true;
                for (int i = 0; i < placed.Count; i++)
                {
                    float required = RequiredAngularGap(envelope, orbitR, placed[i].Envelope, placed[i].OrbitRadius);
                    float delta = AngularDelta(angle, placed[i].AngleRad);
                    if (delta < required) { ok = false; break; }
                }
                if (ok) return NormalizeAngle(angle);
            }

            // Если совсем туго, вернём нормализованный вариант после всех сдвигов — визуально всё равно разъедется.
            return NormalizeAngle(angle);
        }

        private static void RegisterPlaced(int starId, int orbitIndex, float angleRad, float orbitR, float envelope)
        {
            if (!PlacedByStar.TryGetValue(starId, out var list))
            {
                list = new List<PlacedPlanet>(8);
                PlacedByStar[starId] = list;
            }
            list.Add(new PlacedPlanet
            {
                OrbitIndex = orbitIndex,
                AngleRad = NormalizeAngle(angleRad),
                OrbitRadius = Mathf.Max(orbitR, 0.0001f),
                Envelope = Mathf.Max(envelope, 0f)
            });
        }

        // Минимально требуемая угловая щель между двумя планетами, чтобы их «облака» (планета+луны) не перекрывались визуально.
        // При малых углах ширина дуги ≈ R * θ, поэтому θ_min ≈ (E1 / R1 + E2 / R2)/2 + базовая добавка.
        private static float RequiredAngularGap(float env1, float r1, float env2, float r2)
        {
            float e1 = env1 / Mathf.Max(r1, 0.0001f);
            float e2 = env2 / Mathf.Max(r2, 0.0001f);
            return MinAngularGapBase + 0.5f * (e1 + e2);
        }

        private static float AngularDelta(float a, float b)
        {
            // Возвращаем |a-b| в радианах в диапазоне [0 .. π]
            float da = Mathf.Abs(Mathf.DeltaAngle(a * Mathf.Rad2Deg, b * Mathf.Rad2Deg)) * Mathf.Deg2Rad;
            return da;
        }

        private static float NormalizeAngle(float a)
        {
            float twoPi = Mathf.PI * 2f;
            a %= twoPi;
            if (a < 0f) a += twoPi;
            return a;
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
                // масштабируем к [0,1)
                return (x & 0xFFFFFFu) / 16777216f; // 2^24
            }
        }
    }
}
