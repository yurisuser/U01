using System;
using System.Collections.Generic;
using _Project.Scripts.Galaxy.Data;
using UnityEngine;

namespace _Project.Scripts.Galaxy.Generation
{
    public static class PlanetOrbitCreator
    {
        // ==== ТЮНИНГ (если захочешь — правь числа здесь) ====

        // Внутренняя граница по размеру звезды (первая допустимая орбита 1-based)
        private static int InnerCutoff(EStarSize size) => size switch
        {
            EStarSize.Dwarf      => 1,
            EStarSize.Normal     => 2,
            EStarSize.Giant      => 3,
            EStarSize.Supergiant => 4,
            _ => 2
        };

        // Внешняя граница по типу звезды (последняя допустимая орбита 1-based)
        private static int OuterLimit(EStarType type) => type switch
        {
            EStarType.Red     => 18,
            EStarType.Orange  => 18,
            EStarType.Yellow  => 22,
            EStarType.White   => 20,
            EStarType.Blue    => 14,
            EStarType.Neutron => 8,
            EStarType.Black   => 8,
            _ => 20
        };

        // Минимальный разрыв между занятыми орбитами (в «ячейках»)
        private static int MinGap(EStarSize size) => size switch
        {
            EStarSize.Dwarf      => 1,
            EStarSize.Normal     => 2,
            EStarSize.Giant      => 3,
            EStarSize.Supergiant => 4,
            _ => 2
        };

        // Диапазоны количества планет по типу (база)
        private static (int min, int max) BasePlanetCount(EStarType type) => type switch
        {
            EStarType.Red     => (3, 7),
            EStarType.Orange  => (3, 6),
            EStarType.Yellow  => (2, 5),
            EStarType.White   => (1, 4),
            EStarType.Blue    => (0, 3),
            EStarType.Neutron => (0, 1),
            EStarType.Black   => (0, 1),
            _ => (2, 5)
        };

        // Модификатор по размеру
        private static int CountModifier(EStarSize size) => size switch
        {
            EStarSize.Dwarf      => +2,
            EStarSize.Normal     => 0,
            EStarSize.Giant      => -1,
            EStarSize.Supergiant => -2,
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
