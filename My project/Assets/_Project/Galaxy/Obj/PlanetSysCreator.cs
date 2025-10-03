using System;
using System.Collections.Generic;

namespace _Project.Galaxy.Obj
{
    /// <summary>
    /// Собирает PlanetSys по орбитам: накапливает луны и выдаёт снапшот.
    /// Вызывай ResetPerSystem() при старте генерации каждой звёздной системы.
    /// </summary>
    public static class PlanetSysCreator
    {
        // кэш в рамках ТЕКУЩЕЙ звёздной системы
        // ключ = номер орбиты планеты
        private static readonly Dictionary<int, List<Moon>> MoonsByOrbit = new();
        private static readonly Dictionary<int, float> OrbitAngleDegByOrbit = new();

        /// <summary>Сбрасывай при начале новой звёздной системы.</summary>
        public static void ResetPerSystem()
        {
            MoonsByOrbit.Clear();
            OrbitAngleDegByOrbit.Clear();
        }

        /// <summary>
        /// Добавляет луну к орбите планеты и возвращает текущий PlanetSys-снимок
        /// (MotherStar, OrbitIndex, OrbitPosition, Moons[]).
        /// </summary>
        public static PlanetSys Create(Star motherStar, int planetOrbit, Planet planet, Moon moon)
        {
            // 1) накопим луны по орбите
            if (!MoonsByOrbit.TryGetValue(planetOrbit, out var list))
            {
                list = new List<Moon>(4);
                MoonsByOrbit[planetOrbit] = list;
            }
            list.Add(moon);

            // 2) фиксируем угловую позицию орбиты один раз
            if (!OrbitAngleDegByOrbit.TryGetValue(planetOrbit, out float angleDeg))
            {
                angleDeg = UnityEngine.Random.Range(0f, 360f);
                OrbitAngleDegByOrbit[planetOrbit] = angleDeg;
            }

            // 3) возвращаем снапшот по этой орбите (с уже накопленными лунами)
            return new PlanetSys
            {
                MotherStar    = motherStar,
                OrbitIndex    = planetOrbit,
                OrbitPosition = angleDeg,
                Moons         = list.ToArray()
            };
        }

        /// <summary>Финальный снимок по конкретной орбите (если нужно вне цикла лун).</summary>
        public static PlanetSys FinalizeOrbit(Star motherStar, int planetOrbit)
        {
            var moons = MoonsByOrbit.TryGetValue(planetOrbit, out var list) ? list.ToArray() : Array.Empty<Moon>();
            float angleDeg = OrbitAngleDegByOrbit.TryGetValue(planetOrbit, out var a) ? a : 0f;

            return new PlanetSys
            {
                MotherStar    = motherStar,
                OrbitIndex    = planetOrbit,
                OrbitPosition = angleDeg,
                Moons         = moons
            };
        }

        /// <summary>Финальные снимки по всем орбитам текущей системы, отсортированные по номеру.</summary>
        public static PlanetSys[] FinalizeAllOrbits(Star motherStar)
        {
            var res = new List<PlanetSys>(MoonsByOrbit.Count);
            foreach (var kv in MoonsByOrbit)
            {
                int orbit = kv.Key;
                float angleDeg = OrbitAngleDegByOrbit.TryGetValue(orbit, out var a) ? a : 0f;

                res.Add(new PlanetSys
                {
                    MotherStar    = motherStar,
                    OrbitIndex    = orbit,
                    OrbitPosition = angleDeg,
                    Moons         = kv.Value.ToArray()
                });
            }
            res.Sort((a, b) => a.OrbitIndex.CompareTo(b.OrbitIndex));
            return res.ToArray();
        }
    }
}
