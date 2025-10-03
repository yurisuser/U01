using System;
using System.Collections.Generic;
using UnityEngine;
using _Project.Galaxy.Obj;

namespace _Project.Galaxy
{
    // Генератор дискретных орбит планет для звезды.
    // Выход: массив индексов орбит (без радиусов/физики).
    public static class PlanetOrbitCreator
    {
        // Параметры сетки орбит (индексы условных колец)
        private const int InnerForbiddenOrbit = 2;   // 0..1 заняты звёздной короной/опасной зоной
        private const int MaxOrbitIndex       = 40;  // максимальный индекс орбиты
        private const int MinOrbitGap         = 1;   // минимальный зазор между планетами (в индексах)

        // Вероятность одного «большого разрыва» (например, пояс астероидов)
        private const float BigGapChance     = 0.25f;
        private const int   BigGapExtraSpace = 2;

        public static int[] Create(Star star)
        {
            int targetCount = SamplePlanetCount(star);
            if (targetCount <= 0) return Array.Empty<int>();

            List<int> orbits = new List<int>(targetCount);

            int nextOrbit = Mathf.Max(InnerForbiddenOrbit, 0);

            bool willUseBigGap = UnityEngine.Random.value < BigGapChance;
            int bigGapAfterIdx = willUseBigGap && targetCount >= 3
                ? UnityEngine.Random.Range(1, targetCount - 1)
                : -1;

            for (int i = 0; i < targetCount && nextOrbit <= MaxOrbitIndex; i++)
            {
                int noise = UnityEngine.Random.Range(0, 2); // 0..1
                int chosenOrbit = nextOrbit + noise;

                if (orbits.Count > 0)
                {
                    int last = orbits[orbits.Count - 1];
                    if (chosenOrbit - last < MinOrbitGap)
                        chosenOrbit = last + MinOrbitGap;
                }

                if (chosenOrbit > MaxOrbitIndex)
                    break;

                orbits.Add(chosenOrbit);

                int step = MinOrbitGap + 1 + UnityEngine.Random.Range(0, 2); // 2..3
                if (i == bigGapAfterIdx)
                    step += BigGapExtraSpace + UnityEngine.Random.Range(0, 2);

                nextOrbit = chosenOrbit + step;
            }

            return orbits.ToArray();
        }

        private static int SamplePlanetCount(Star star)
        {
            // Базово 3..8, с лёгкой вариативностью и влиянием «массы» звезды (если есть)
            int baseMin = 3;
            int baseMax = 8;

            float massFactor = 1f;
            try
            {
                // Допускаем наличие свойства Mass у Star. Если его нет — используем 1.
                float m = (star.mass == 0f) ? 1f : star.mass;
                massFactor = Mathf.Clamp(m, 0.7f, 1.3f);
            }
            catch
            {
                massFactor = 1f;
            }

            int minC = Mathf.RoundToInt(baseMin * massFactor);
            int maxC = Mathf.RoundToInt(baseMax * massFactor);
            if (minC > maxC) (minC, maxC) = (maxC, minC);

            int count = UnityEngine.Random.Range(minC, maxC + 1);

            // Небольшой шанс уменьшить/увеличить на 1
            if (UnityEngine.Random.value < 0.15f && count > 0) count -= 1;
            if (UnityEngine.Random.value < 0.15f) count += 1;

            return Mathf.Clamp(count, 0, 10);
        }
    }
}
