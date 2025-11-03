using System;
using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using UnityEngine;

namespace _Project.Scripts.Galaxy.Generation
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
        // (Stone/Desert/Toxic/Frozen/Ocean/GasGiant/IceGiant/Blasted/Lava в твоём EPlanetType наборе — лунные типы здесь)
        private static (EMoonType t, int w)[] WeightsByPlanetType(EPlanetType p) => p switch
        {
            EPlanetType.GasGiant => new[]
            {
                (EMoonType.Ice, 40), (EMoonType.Ocean, 10), (EMoonType.Stone, 10),
                (EMoonType.Desert, 5), (EMoonType.Lava, 5), (EMoonType.Toxic, 10), (EMoonType.Blasted, 20)
            },
            EPlanetType.IceGiant => new[]
            {
                (EMoonType.Ice, 45), (EMoonType.Ocean, 15), (EMoonType.Stone, 10),
                (EMoonType.Desert, 5), (EMoonType.Lava, 5), (EMoonType.Toxic, 5), (EMoonType.Blasted, 15)
            },
            EPlanetType.Ocean => new[]
            {
                (EMoonType.Ocean, 25), (EMoonType.Ice, 25), (EMoonType.Stone, 20),
                (EMoonType.Desert, 10), (EMoonType.Lava, 5), (EMoonType.Toxic, 5), (EMoonType.Blasted, 10)
            },
            EPlanetType.Stone => new[]
            {
                (EMoonType.Stone, 35), (EMoonType.Ice, 25), (EMoonType.Desert, 15),
                (EMoonType.Lava, 10), (EMoonType.Toxic, 5), (EMoonType.Ocean, 5), (EMoonType.Blasted, 5)
            },
            EPlanetType.Desert => new[]
            {
                (EMoonType.Desert, 35), (EMoonType.Stone, 25), (EMoonType.Ice, 20),
                (EMoonType.Lava, 5), (EMoonType.Toxic, 5), (EMoonType.Ocean, 3), (EMoonType.Blasted, 7)
            },
            EPlanetType.Frozen => new[]
            {
                (EMoonType.Ice, 45), (EMoonType.Stone, 20), (EMoonType.Ocean, 10),
                (EMoonType.Desert, 10), (EMoonType.Lava, 3), (EMoonType.Toxic, 2), (EMoonType.Blasted, 10)
            },
            EPlanetType.Lava => new[]
            {
                (EMoonType.Lava, 35), (EMoonType.Stone, 25), (EMoonType.Desert, 15),
                (EMoonType.Ice, 10), (EMoonType.Toxic, 5), (EMoonType.Ocean, 3), (EMoonType.Blasted, 7)
            },
            EPlanetType.Toxic => new[]
            {
                (EMoonType.Toxic, 30), (EMoonType.Stone, 25), (EMoonType.Desert, 20),
                (EMoonType.Ice, 10), (EMoonType.Ocean, 5), (EMoonType.Lava, 5), (EMoonType.Blasted, 5)
            },
            EPlanetType.Blasted => new[]
            {
                (EMoonType.Blasted, 40), (EMoonType.Stone, 20), (EMoonType.Desert, 15),
                (EMoonType.Ice, 10), (EMoonType.Lava, 10), (EMoonType.Toxic, 5), (EMoonType.Ocean, 0)
            },
            _ => new[]
            {
                (EMoonType.Stone, 30), (EMoonType.Ice, 30), (EMoonType.Desert, 15),
                (EMoonType.Ocean, 10), (EMoonType.Lava, 5), (EMoonType.Toxic, 5), (EMoonType.Blasted, 5)
            }
        };

        // Модификатор по ЗВЕЗДЕ (жёсткие — усиливают Blasted/Toxic, душат Ocean)
        private static void ApplyStarHazard(EStarType eStar, ref (EMoonType t, int w)[] weights)
        {
            if (eStar == EStarType.Neutron || eStar == EStarType.Black)
            {
                for (int i = 0; i < weights.Length; i++)
                {
                    if (weights[i].t == EMoonType.Blasted) weights[i].w = Mathf.Min(100, weights[i].w + 20);
                    if (weights[i].t == EMoonType.Toxic)   weights[i].w = Mathf.Min(100, weights[i].w + 10);
                    if (weights[i].t == EMoonType.Ocean)   weights[i].w = Mathf.Max(0,   weights[i].w - 20);
                }
            }
            else if (eStar == EStarType.Blue || eStar == EStarType.White)
            {
                for (int i = 0; i < weights.Length; i++)
                {
                    if (weights[i].t == EMoonType.Blasted) weights[i].w = Mathf.Min(100, weights[i].w + 10);
                    if (weights[i].t == EMoonType.Ocean)   weights[i].w = Mathf.Max(0,   weights[i].w - 10);
                }
            }
        }

        // Модификатор по ОРБИТЕ ЛУНЫ (близко — больше Lava/Stone/Desert; далеко — Icee/Ocean/Blasted)
        private static void ApplyMoonOrbitBias(int moonOrbitIndex, int planetOrbitIndex, ref (EMoonType t, int w)[] weights)
        {
            // u ~ «удалённость луны»: чем больше индекс, тем холоднее
            float u = Mathf.Clamp01((moonOrbitIndex - 1) / 8f); // 1..~9 нормализуем в 0..1

            for (int i = 0; i < weights.Length; i++)
            {
                switch (weights[i].t)
                {
                    case EMoonType.Lava:
                        weights[i].w = Mathf.RoundToInt(Mathf.Clamp(weights[i].w * (1.0f + 0.6f * (1f - u)), 0f, 100f));
                        break;
                    case EMoonType.Stone:
                    case EMoonType.Desert:
                        weights[i].w = Mathf.RoundToInt(Mathf.Clamp(weights[i].w * (1.0f + 0.3f * (1f - u)), 0f, 100f));
                        break;
                    case EMoonType.Ice:
                    case EMoonType.Ocean:
                        weights[i].w = Mathf.RoundToInt(Mathf.Clamp(weights[i].w * (1.0f + 0.4f * u), 0f, 100f));
                        break;
                    case EMoonType.Blasted:
                        // чуть чаще на внешних орбитах
                        weights[i].w = Mathf.RoundToInt(Mathf.Clamp(weights[i].w * (1.0f + 0.2f * u), 0f, 100f));
                        break;
                    case EMoonType.Toxic:
                        // слабая зависимость
                        break;
                }
            }
        }

        // Веса размеров лун по КЛАССУ РАЗМЕРА ПЛАНЕТЫ
        private static int[] SizeWeights(PSize host)
        {
            // порядок: Tiny, Small, Medium, Large (см. EMoonSize):contentReference[oaicite:4]{index=4}
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
            EMoonType mType = PickWeighted(w);

            // 2) Размер
            var hostClass = ClassifyPlanet(Mathf.Max(planet.Radius, 0.1f));
            EMoonSize mSize = PickSizeWeighted(SizeWeights(hostClass));

            // 3) Сборка Moon (минимально необходимое; остальное ты уже сам довесишь при надобности)
            return new Moon
            {
                Uid = UIDService.Create(EntityType.Moon),
                NameId = -1,
                Type = mType,              // из твоего enum EMoonType :contentReference[oaicite:5]{index=5}
                Size = mSize,              // из твоего enum EMoonSize :contentReference[oaicite:6]{index=6}
                OrbitIndex = moonOrbitIndex
                // Остальные поля (Mass, Radius, OrbitDistance, …) оставляю по умолчанию — под твои генераторы.
            };
        }

        // =========================
        // ВНУТРЕНКА
        // =========================
        private static EMoonType PickWeighted((EMoonType t, int w)[] items)
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

        private static EMoonSize PickSizeWeighted(int[] w)
        {
            int total = 0;
            for (int i = 0; i < w.Length; i++) total += Mathf.Max(0, w[i]);
            int r = UnityEngine.Random.Range(0, Math.Max(1, total));
            int acc = 0;

            // порядок должен соответствовать EMoonSize: Tiny, Small, Medium, Large :contentReference[oaicite:7]{index=7}
            if ((acc += w[0]) > r) return EMoonSize.Tiny;
            if ((acc += w[1]) > r) return EMoonSize.Small;
            if ((acc += w[2]) > r) return EMoonSize.Medium;
            if ((acc += w[2]) > r) return EMoonSize.Medium;
            return EMoonSize.Large;
        }
    }
}
