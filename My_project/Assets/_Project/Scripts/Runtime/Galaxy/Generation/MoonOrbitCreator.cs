using System;
using System.Collections.Generic;
using _Project.Scripts.Galaxy.Data;
using UnityEngine;

namespace _Project.Scripts.Galaxy.Generation
{
    /// <summary>
    /// Возвращает массив занятых ЛУНАМИ орбит (1-based индексы) для заданной планеты.
    /// Веса количества и удалённости зависят от типа планеты и её радиуса.
    /// </summary>
    public static class MoonOrbitCreator
    {
        // ===================== ТЮНИНГ (всё сверху) =====================

        // Классификация размера планеты по радиусу (в земных радиусах)
        private enum SizeClass { Small, Medium, Large }
        private static SizeClass Classify(float radiusRe)
        {
            if (radiusRe >= 4f) return SizeClass.Large;   // газ/ледяные гиганты и супер-земли
            if (radiusRe >= 1.5f) return SizeClass.Medium;
            return SizeClass.Small;
        }

        // Внутренняя запретная зона (inner cutoff) для лун — ближе «жарко/приливно», чаще кольца
        private static int InnerCutoff(EPlanetType type, SizeClass sc) => type switch
        {
            EPlanetType.GasGiant => 2,    // гиганты — первая «надёжная» орбита дальше
            EPlanetType.IceGiant => 2,
            _ => sc == SizeClass.Small ? 1 : 2 // крупные супер-земли/средние — немного дальше
        };

        // Внешняя граница лун — зависит от типа планеты и её размера (как суррогат Хилловой сферы)
        private static int OuterLimit(EPlanetType type, SizeClass sc)
        {
            int baseLimit = type switch
            {
                EPlanetType.GasGiant => 14,
                EPlanetType.IceGiant => 11,
                EPlanetType.Ocean    => 7,
                EPlanetType.Stone    => 6,
                EPlanetType.Desert   => 6,
                EPlanetType.Toxic    => 6,
                EPlanetType.Frozen   => 7,
                EPlanetType.Blasted  => 5,
                _ => 6
            };
            // крупнее планета — чуть дальше тянется «зона удержания»
            int bonus = sc switch { SizeClass.Large => 2, SizeClass.Medium => 1, _ => 0 };
            return Mathf.Max(InnerCutoff(type, sc) + 1, baseLimit + bonus);
        }

        // Минимальный разрыв между орбитами лун (в «ячейках»)
        private static int MinGap(EPlanetType type, SizeClass sc) => type switch
        {
            EPlanetType.GasGiant => 1, // много лун — разрешаем плотнее
            EPlanetType.IceGiant => 1,
            _ => sc == SizeClass.Small ? 2 : 1
        };

        // ===== ВЕСА ДЛЯ КОЛИЧЕСТВА ЛУН (0–100) =====
        // Берём один из «коридоров» численности с указанным весом: low / mid / high
        private struct CountProfile { public Vector2Int Low; public int WLow; public Vector2Int Mid; public int WMid; public Vector2Int High; public int WHigh; }

        private static CountProfile CountByType(EPlanetType t, SizeClass sc)
        {
            // дефолт для каменных/океанических/пустынных/токсичных/замёрзших
            var common = new CountProfile
            {
                Low  = new Vector2Int(0, 1),  WLow  = 55,
                Mid  = new Vector2Int(2, 3),  WMid  = 35,
                High = new Vector2Int(4, 5),  WHigh = 10
            };

            switch (t)
            {
                case EPlanetType.Stone:
                case EPlanetType.Desert:
                case EPlanetType.Toxic:
                case EPlanetType.Blasted:
                    // у малых чаще 0–1, у средних иногда 2–3
                    if (sc == SizeClass.Small)  return new CountProfile { Low=new Vector2Int(0,1), WLow=70, Mid=new Vector2Int(2,2), WMid=25, High=new Vector2Int(3,3), WHigh=5 };
                    if (sc == SizeClass.Medium) return new CountProfile { Low=new Vector2Int(0,1), WLow=45, Mid=new Vector2Int(2,3), WMid=45, High=new Vector2Int(4,4), WHigh=10 };
                    return new CountProfile { Low=new Vector2Int(1,2), WLow=40, Mid=new Vector2Int(2,3), WMid=45, High=new Vector2Int(4,5), WHigh=15 };

                case EPlanetType.Ocean:
                case EPlanetType.Frozen:
                    if (sc == SizeClass.Small)  return new CountProfile { Low=new Vector2Int(0,1), WLow=60, Mid=new Vector2Int(2,3), WMid=35, High=new Vector2Int(4,4), WHigh=5 };
                    if (sc == SizeClass.Medium) return new CountProfile { Low=new Vector2Int(1,2), WLow=45, Mid=new Vector2Int(2,3), WMid=45, High=new Vector2Int(4,5), WHigh=10 };
                    return new CountProfile { Low=new Vector2Int(1,2), WLow=35, Mid=new Vector2Int(2,4), WMid=50, High=new Vector2Int(5,6), WHigh=15 };

                case EPlanetType.GasGiant:
                    // гиганты: обычно 6–12, иногда 3–5, редко 13–16
                    return new CountProfile
                    {
                        Low  = new Vector2Int(3, 5),  WLow  = 25,
                        Mid  = new Vector2Int(6, 12), WMid  = 60,
                        High = new Vector2Int(13,16), WHigh = 15
                    };

                case EPlanetType.IceGiant:
                    return new CountProfile
                    {
                        Low  = new Vector2Int(2, 4),  WLow  = 40,
                        Mid  = new Vector2Int(5, 8),  WMid  = 50,
                        High = new Vector2Int(9, 11), WHigh = 10
                    };

                default:
                    return common;
            }
        }

        // ===== ВЕСА ДЛЯ УДАЛЁННОСТИ (0–100) =====
        // Функция весов по индексу орбиты; разные «профили» под типы.
        private static float OrbitWeight(EPlanetType t, int orbit, int inner, int outer)
        {
            // нормализуем 0..1
            float u = (outer > inner) ? Mathf.InverseLerp(inner, outer, orbit) : 0.5f;

            switch (t)
            {
                case EPlanetType.GasGiant:
                    // пик в средней зоне (u ≈ 0.5), приглушаем крайние
                    return 40f + 60f * Mathf.Exp(-Mathf.Pow((u - 0.55f) / 0.2f, 2f));
                case EPlanetType.IceGiant:
                    // пик чуть ближе к планете
                    return 35f + 65f * Mathf.Exp(-Mathf.Pow((u - 0.45f) / 0.22f, 2f));
                case EPlanetType.Stone:
                case EPlanetType.Desert:
                case EPlanetType.Toxic:
                case EPlanetType.Blasted:
                    // у каменных — ближе лучше; плавный спад к внешним
                    return 80f - 60f * u;
                case EPlanetType.Ocean:
                case EPlanetType.Frozen:
                    // двухгорбый: ближняя и средняя зоны
                    float a = 60f * Mathf.Exp(-Mathf.Pow((u - 0.25f) / 0.18f, 2f));
                    float b = 40f * Mathf.Exp(-Mathf.Pow((u - 0.55f) / 0.22f, 2f));
                    return 20f + a + b;
                default:
                    return 50f;
            }
        }

        // ===================== ПУБЛИЧНО =====================

        public static int[] Create(Planet planet)
        {
            // Определяем класс размера планеты по радиусу (земные радиусы)
            var sc = Classify(Mathf.Max(planet.Radius, 0.1f)); // защита от нуля

            int inner = InnerCutoff(planet.Type, sc);
            int outer = OuterLimit(planet.Type, sc);
            if (outer <= inner) return Array.Empty<int>();

            // Сколько лун хотим: выбираем один из коридоров по весам, затем случай в его пределах
            var prof = CountByType(planet.Type, sc);
            int want = PickCountWeighted(prof);
            if (want <= 0) return Array.Empty<int>();

            // Кандидаты и отбор с зазором + рулеткой по весам удалённости
            int gap = MinGap(planet.Type, sc);
            var picked = new List<int>(want);
            var candidates = new List<int>(outer - inner + 1);
            for (int o = inner; o <= outer; o++) candidates.Add(o);

            // до тех пор, пока не наберём нужное или не кончатся кандидаты
            for (int attempts = 0; attempts < 256 && picked.Count < want && candidates.Count > 0; attempts++)
            {
                int choice = PickOrbitWeighted(candidates, i => OrbitWeight(planet.Type, i, inner, outer));

                if (IsFarEnough(choice, picked, gap))
                {
                    picked.Add(choice);
                    // вычистим «запрещённые» орбиты вокруг
                    RemoveNeighborhood(candidates, choice, gap - 1);
                }
                else
                {
                    // если близко — просто убираем этот кандидат и пробуем дальше
                    candidates.Remove(choice);
                }
            }

            picked.Sort();
            return picked.ToArray();
        }

        // ===================== ВНУТРЕНКА =====================

        private static bool IsFarEnough(int orbit, List<int> taken, int gap)
        {
            for (int i = 0; i < taken.Count; i++)
                if (Mathf.Abs(taken[i] - orbit) < gap) return false;
            return true;
        }

        private static void RemoveNeighborhood(List<int> pool, int center, int radius)
        {
            if (radius <= 0) { pool.Remove(center); return; }
            for (int i = pool.Count - 1; i >= 0; i--)
                if (Mathf.Abs(pool[i] - center) <= radius) pool.RemoveAt(i);
        }

        private static int PickCountWeighted(CountProfile p)
        {
            int wSum = p.WLow + p.WMid + p.WHigh;
            int r = UnityEngine.Random.Range(0, Mathf.Max(1, wSum));
            if (r < p.WLow) return UnityEngine.Random.Range(p.Low.x,  p.Low.y  + 1);
            r -= p.WLow;
            if (r < p.WMid) return UnityEngine.Random.Range(p.Mid.x,  p.Mid.y  + 1);
            return UnityEngine.Random.Range(p.High.x, p.High.y + 1);
        }

        private static int PickOrbitWeighted(List<int> candidates, Func<int, float> weightFn)
        {
            // рулетка по весам кандидатов
            float total = 0f;
            for (int i = 0; i < candidates.Count; i++) total += Mathf.Max(0.0001f, weightFn(candidates[i]));
            float r = UnityEngine.Random.Range(0f, Mathf.Max(0.0001f, total));
            float acc = 0f;
            for (int i = 0; i < candidates.Count; i++)
            {
                acc += Mathf.Max(0.0001f, weightFn(candidates[i]));
                if (r <= acc) return candidates[i];
            }
            return candidates[candidates.Count - 1];
        }
    }
}
