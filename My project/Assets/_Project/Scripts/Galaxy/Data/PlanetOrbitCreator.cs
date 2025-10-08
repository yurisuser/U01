using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Galaxy.Data
{
    public static class PlanetOrbitCreator
    {
        // ==== ТЮНИНГ (если захочешь — правь числа здесь) ====

        // Внутренняя граница по размеру звезды (первая допустимая орбита 1-based)
        private static int InnerCutoff(StarSize size) => size switch
        {
            StarSize.Dwarf      => 1,
            StarSize.Normal     => 2,
            StarSize.Giant      => 3,
            StarSize.Supergiant => 4,
            _ => 2
        };

        // Внешняя граница по типу звезды (последняя допустимая орбита 1-based)
        private static int OuterLimit(StarType type) => type switch
        {
            StarType.Red     => 18,
            StarType.Orange  => 18,
            StarType.Yellow  => 22,
            StarType.White   => 20,
            StarType.Blue    => 14,
            StarType.Neutron => 8,
            StarType.Black   => 8,
            _ => 20
        };

        // Минимальный разрыв между занятыми орбитами (в «ячейках»)
        private static int MinGap(StarSize size) => size switch
        {
            StarSize.Dwarf      => 1,
            StarSize.Normal     => 2,
            StarSize.Giant      => 3,
            StarSize.Supergiant => 4,
            _ => 2
        };

        // Диапазоны количества планет по типу (база)
        private static (int min, int max) BasePlanetCount(StarType type) => type switch
        {
            StarType.Red     => (3, 7),
            StarType.Orange  => (3, 6),
            StarType.Yellow  => (2, 5),
            StarType.White   => (1, 4),
            StarType.Blue    => (0, 3),
            StarType.Neutron => (0, 1),
            StarType.Black   => (0, 1),
            _ => (2, 5)
        };

        // Модификатор по размеру
        private static int CountModifier(StarSize size) => size switch
        {
            StarSize.Dwarf      => +2,
            StarSize.Normal     => 0,
            StarSize.Giant      => -1,
            StarSize.Supergiant => -2,
            _ => 0
        };

        // ==== ПУБЛИЧНО ====

        /// <summary>
        /// Возвращает массив занятых планетами орбит (1-based индексы).
        /// </summary>
        public static int[] Create(Star star)
        {
            // Границы и ограничения
            int inner = InnerCutoff(star.size);
            int outer = OuterLimit(star.type);
            if (outer < inner) return Array.Empty<int>();

            // Сколько планет хотим
            var (minBase, maxBase) = BasePlanetCount(star.type);
            int delta = CountModifier(star.size);
            int wantMin = Mathf.Max(0, minBase + delta);
            int wantMax = Mathf.Max(wantMin, maxBase + delta);

            int want = UnityEngine.Random.Range(wantMin, wantMax + 1);
            if (want == 0) return Array.Empty<int>();

            // Кандидаты и жадный отбор с зазором
            int gap = MinGap(star.size);
            var candidates = new List<int>(outer - inner + 1);
            for (int o = inner; o <= outer; o++) candidates.Add(o);

            var picked = new List<int>(want);
            // перемешаем кандидатов, чтобы не брать всегда одинаково
            Shuffle(candidates);

            foreach (int o in candidates)
            {
                if (IsFarEnough(o, picked, gap))
                {
                    picked.Add(o);
                    if (picked.Count >= want) break;
                }
            }

            picked.Sort();
            return picked.ToArray();
        }

        // ==== ВНУТРЕНКА ====

        private static bool IsFarEnough(int orbit, List<int> taken, int gap)
        {
            for (int i = 0; i < taken.Count; i++)
            {
                if (Mathf.Abs(taken[i] - orbit) < gap) return false;
            }
            return true;
        }

        private static void Shuffle(List<int> list)
        {
            // простой Фишер–Йетс на Unity Random
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
