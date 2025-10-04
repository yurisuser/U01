using System;
using System.Collections.Generic;
using UnityEngine;
using static _Project.CONSTANT.GALAXY; // OrbitSlots и прочие константы

namespace _Project.Scripts.Galaxy.Data
{
    public static class MoonOrbitCreator
    {
        public static int[] Create(Planet planet)
        {
            // 1) Сколько лун хотим (грубая, но играбельная логика)
            int desired = EstimateMoonCount(planet);

            if (desired <= 0) return Array.Empty<int>();
            desired = Mathf.Min(desired, Mathf.Max(1, OrbitSlots)); // безопасность

            // 2) Выбираем УНИКАЛЬНЫЕ орбиты в диапазоне [1..  OrbitSlots]
            //    (можно без «зазоров», у тебя круговые орбиты без физики)
            var picks = new HashSet<int>();
            for (int safety = 0; safety < 256 && picks.Count < desired; safety++)
            {
                int o = UnityEngine.Random.Range(1, OrbitSlots + 1); // верхняя не включается, поэтому +1
                picks.Add(o);
            }

            // 3) Сортируем, чтобы было красиво и стабильно
            var result = new List<int>(picks);
            result.Sort();
            return result.ToArray();
        }

        // ----------------- helpers -----------------

        private static int EstimateMoonCount(Planet p)
        {
            string t = (p.Type ?? "rocky").ToLowerInvariant();

            // Базовые коридоры по типам
            (int min, int max) range = t switch
            {
                "gas_giant" => (3, 7),
                "ice_giant" => (2, 5),
                "rocky"     => (0, 2),
                "ocean"     => (0, 2),
                "desert"    => (0, 2),
                "lava"      => (0, 1),
                "toxic"     => (0, 1),
                "frozen"    => (0, 2),
                "dwarf"     => (0, 1),
                _           => (0, 2)
            };

            // Небольшая поправка от массы: чем тяжелее — тем больше шанс на лишнюю луну
            // Масса у тебя в условных ед.; считаем «тяжёлой» > 1.5
            int bonus = (p.Mass > 3f) ? 2 : (p.Mass > 1.5f ? 1 : 0);

            int min = range.min;
            int max = range.max + bonus;

            // Не вылезаем за количество доступных «слотов» орбит
            max = Mathf.Clamp(max, 0, Mathf.Max(0, OrbitSlots));

            if (max <= 0) return 0;
            if (min > max) min = max;

            return UnityEngine.Random.Range(min, max + 1); // верхняя граница включительно
        }
    }
}
