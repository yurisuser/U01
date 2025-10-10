using System;
using UnityEngine;

namespace _Project.Scripts.Galaxy.Data
{
    public static class MoonCreator
    {
        // Классификация размера планеты по радиусу (в земных радиусах)
        private enum PSize { Small, Medium, Large }
        private static PSize ClassifyPlanet(float rRe)
        {
            if (rRe >= 4f) return PSize.Large;
            if (rRe >= 1.5f) return PSize.Medium;
            return PSize.Small;
        }

        // Базовые веса типов лун по ТИПУ ПЛАНЕТЫ
        // (Stone/Desert/Toxic/Frozen/Ocean/GasGiant/IceGiant/Blasted/Lava в твоём PlanetType наборе — лунные типы здесь)
        private static (MoonType t, int w)[] WeightsByPlanetType(PlanetType p) => p switch
        {
            PlanetType.GasGiant => new[]
            {
                (MoonType.Ice, 40), (MoonType.Ocean, 10), (MoonType.Stone, 10),
                (MoonType.Desert, 5), (MoonType.Lava, 5), (MoonType.Toxic, 10), (MoonType.Blasted, 20)
            },
            PlanetType.IceGiant => new[]
            {
                (MoonType.Ice, 45), (MoonType.Ocean, 15), (MoonType.Stone, 10),
                (MoonType.Desert, 5), (MoonType.Lava, 5), (MoonType.Toxic, 5), (MoonType.Blasted, 15)
            },
            PlanetType.Ocean => new[]
            {
                (MoonType.Ocean, 25), (MoonType.Ice, 25), (MoonType.Stone, 20),
                (MoonType.Desert, 10), (MoonType.Lava, 5), (MoonType.Toxic, 5), (MoonType.Blasted, 10)
            },
            PlanetType.Stone => new[]
            {
                (MoonType.Stone, 35), (MoonType.Ice, 25), (MoonType.Desert, 15),
                (MoonType.Lava, 10), (MoonType.Toxic, 5), (MoonType.Ocean, 5), (MoonType.Blasted, 5)
            },
            PlanetType.Desert => new[]
            {
                (MoonType.Desert, 35), (MoonType.Stone, 25), (MoonType.Ice, 20),
                (MoonType.Lava, 5), (MoonType.Toxic, 5), (MoonType.Ocean, 3), (MoonType.Blasted, 7)
            },
            PlanetType.Frozen => new[]
            {
                (MoonType.Ice, 45), (MoonType.Stone, 20), (MoonType.Ocean, 10),
                (MoonType.Desert, 10), (MoonType.Lava, 3), (MoonType.Toxic, 2), (MoonType.Blasted, 10)
            },
            PlanetType.Lava => new[]
            {
                (MoonType.Lava, 35), (MoonType.Stone, 25), (MoonType.Desert, 15),
                (MoonType.Ice, 10), (MoonType.Toxic, 5), (MoonType.Ocean, 3), (MoonType.Blasted, 7)
            },
            PlanetType.Toxic => new[]
            {
                (MoonType.Toxic, 30), (MoonType.Stone, 25), (MoonType.Desert, 20),
                (MoonType.Ice, 10), (MoonType.Ocean, 5), (MoonType.Lava, 5), (MoonType.Blasted, 5)
            },
            PlanetType.Blasted => new[]
            {
                (MoonType.Blasted, 40), (MoonType.Stone, 20), (MoonType.Desert, 15),
                (MoonType.Ice, 10), (MoonType.Lava, 10), (MoonType.Toxic, 5), (MoonType.Ocean, 0)
            },
            _ => new[]
            {
                (MoonType.Stone, 30), (MoonType.Ice, 30), (MoonType.Desert, 15),
                (MoonType.Ocean, 10), (MoonType.Lava, 5), (MoonType.Toxic, 5), (MoonType.Blasted, 5)
            }
        };

        // Модификатор по ЗВЕЗДЕ (жёсткие — усиливают Blasted/Toxic, душат Ocean)
        private static void ApplyStarHazard(StarType star, ref (MoonType t, int w)[] weights)
        {
            if (star == StarType.Neutron || star == StarType.Black)
            {
                for (int i = 0; i < weights.Length; i++)
                {
                    if (weights[i].t == MoonType.Blasted) weights[i].w = Mathf.Min(100, weights[i].w + 20);
                    if (weights[i].t == MoonType.Toxic)   weights[i].w = Mathf.Min(100, weights[i].w + 10);
                    if (weights[i].t == MoonType.Ocean)   weights[i].w = Mathf.Max(0,   weights[i].w - 20);
                }
            }
            else if (star == StarType.Blue || star == StarType.White)
            {
                for (int i = 0; i < weights.Length; i++)
                {
                    if (weights[i].t == MoonType.Blasted) weights[i].w = Mathf.Min(100, weights[i].w + 10);
                    if (weights[i].t == MoonType.Ocean)   weights[i].w = Mathf.Max(0,   weights[i].w - 10);
                }
            }
        }

        // Модификатор по ОРБИТЕ ЛУНЫ (близко — больше Lava/Stone/Desert; далеко — Icee/Ocean/Blasted)
        private static void ApplyMoonOrbitBias(int moonOrbitIndex, int planetOrbitIndex, ref (MoonType t, int w)[] weights)
        {
            // u ~ «удалённость луны»: чем больше индекс, тем холоднее
            float u = Mathf.Clamp01((moonOrbitIndex - 1) / 8f); // 1..~9 нормализуем в 0..1

            for (int i = 0; i < weights.Length; i++)
            {
                switch (weights[i].t)
                {
                    case MoonType.Lava:
                        weights[i].w = Mathf.RoundToInt(Mathf.Clamp(weights[i].w * (1.0f + 0.6f * (1f - u)), 0f, 100f));
                        break;
                    case MoonType.Stone:
                    case MoonType.Desert:
                        weights[i].w = Mathf.RoundToInt(Mathf.Clamp(weights[i].w * (1.0f + 0.3f * (1f - u)), 0f, 100f));
                        break;
                    case MoonType.Ice:
                    case MoonType.Ocean:
                        weights[i].w = Mathf.RoundToInt(Mathf.Clamp(weights[i].w * (1.0f + 0.4f * u), 0f, 100f));
                        break;
                    case MoonType.Blasted:
                        // чуть чаще на внешних орбитах
                        weights[i].w = Mathf.RoundToInt(Mathf.Clamp(weights[i].w * (1.0f + 0.2f * u), 0f, 100f));
                        break;
                    case MoonType.Toxic:
                        // слабая зависимость
                        break;
                }
            }
        }

        // Веса размеров лун по КЛАССУ РАЗМЕРА ПЛАНЕТЫ
        private static int[] SizeWeights(PSize host)
        {
            // порядок: Tiny, Small, Medium, Large (см. MoonSize):contentReference[oaicite:4]{index=4}
            return host switch
            {
                PSize.Small  => new[] { 50, 35, 15, 0 },  // маленькие планеты — мелкие луны
                PSize.Medium => new[] { 25, 40, 30, 5 },
                PSize.Large  => new[] { 10, 30, 40, 20 }, // гиганты — крупные луны чаще
                _ => new[] { 30, 40, 25, 5 }
            };
        }

        // =========================
        // ПУБЛИЧНЫЙ API
        // =========================
        public static Moon Create(Star star, int planetOrbitIndex, Planet planet, int moonOrbitIndex)
        {
            // 1) Тип
            var w = WeightsByPlanetType(planet.Type);
            ApplyStarHazard(star.type, ref w);
            ApplyMoonOrbitBias(moonOrbitIndex, planetOrbitIndex, ref w);
            MoonType mType = PickWeighted(w);

            // 2) Размер
            var hostClass = ClassifyPlanet(Mathf.Max(planet.Radius, 0.1f));
            MoonSize mSize = PickSizeWeighted(SizeWeights(hostClass));

            // 3) Сборка Moon (минимально необходимое; остальное ты уже сам довесишь при надобности)
            return new Moon
            {
                Name = null,
                Type = mType,              // из твоего enum MoonType :contentReference[oaicite:5]{index=5}
                Size = mSize,              // из твоего enum MoonSize :contentReference[oaicite:6]{index=6}
                OrbitIndex = moonOrbitIndex
                // Остальные поля (Mass, Radius, OrbitDistance, …) оставляю по умолчанию — под твои генераторы.
            };
        }

        // =========================
        // ВНУТРЕНКА
        // =========================
        private static MoonType PickWeighted((MoonType t, int w)[] items)
        {
            int total = 0;
            for (int i = 0; i < items.Length; i++) total += Mathf.Max(0, items[i].w);
            int r = UnityEngine.Random.Range(0, Math.Max(1, total));
            int acc = 0;
            for (int i = 0; i < items.Length; i++)
            {
                acc += Mathf.Max(0, items[i].w);
                if (r < acc) return items[i].t;
            }
            return items[0].t;
        }

        private static MoonSize PickSizeWeighted(int[] w)
        {
            int total = 0;
            for (int i = 0; i < w.Length; i++) total += Mathf.Max(0, w[i]);
            int r = UnityEngine.Random.Range(0, Math.Max(1, total));
            int acc = 0;

            // порядок должен соответствовать MoonSize: Tiny, Small, Medium, Large :contentReference[oaicite:7]{index=7}
            if ((acc += w[0]) > r) return MoonSize.Tiny;
            if ((acc += w[1]) > r) return MoonSize.Small;
            if ((acc += w[2]) > r) return MoonSize.Medium;
            return MoonSize.Large;
        }
    }
}
