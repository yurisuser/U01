﻿using System;
using _Project.Galaxy.Obj;
using UnityEngine;

namespace _Project.Galaxy
{
    public static class GalaxyCreator
    {
        // === настройки ===
        private const int   StarCount                 = 1000;
        private const float GalaxyRadius              = 100f;
        private const float GalaxyStarLayer           = 0f;

        private const float DensityArms               = 4.2f;
        private const float WidthArms                 = 10f;

        private const float MinStarInterval           = 1f;
        private const float CentralBlackHoleIntervalK = 7f;

        private const int   MaxAttemptsPerStar        = 64;

        // внутреннее
        private static float _lastRawX;
        private static float _lastRawY;

        // — точка входа, как просил —
        public static StarSys[] Create()
        {
            return CreateSpiralGalaxy(StarCount, GalaxyStarLayer);
        }

        private static StarSys[] CreateSpiralGalaxy(int count, float zLayer)
        {
            if (count <= 0) return Array.Empty<StarSys>();
            var arr = new StarSys[count];

            // центр
            arr[0] = new StarSys
            {
                GalaxyPosition = new Vector3(0f, 0f, zLayer),
                OldX = 0f,
                OldY = 0f
            };

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

        private static Vector3 PlaceWithMinDistance(
            int index,
            StarSys[] placed,
            Func<Vector3> sampleFunc,
            float baseMinDist,
            float centerExtraK,
            int maxAttempts)
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
