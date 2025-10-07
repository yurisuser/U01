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
            return 4;
        }
    }
}
