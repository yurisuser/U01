using System;
using UnityEngine;

namespace _Project.Scripts.Galaxy.Data
{
    public static class PlanetCreator
    {
        // ========= ТЮНИНГ (всё сверху) =========

        // «Центр» обитаемой зоны по типу звезды — в условных орбитах (1-based)
        // и «ширина» зоны (полуширина).
        private static (int center, int halfWidth) HabZone(StarType t) => t switch
        {
            StarType.Red     => (3, 1),   // близко к звезде
            StarType.Orange  => (5, 1),
            StarType.Yellow  => (7, 1),
            StarType.White   => (9, 2),
            StarType.Blue    => (12, 2),  // дальше
            StarType.Neutron => (2, 0),   // условно
            StarType.Black   => (2, 0),   // условно
            _ => (7, 1)
        };

        // «Снежная линия» — после неё растёт шанс ледяных/газовых гигантов
        private static int SnowLine(StarType t) => t switch
        {
            StarType.Red     => 6,
            StarType.Orange  => 8,
            StarType.Yellow  => 11,
            StarType.White   => 14,
            StarType.Blue    => 18,
            StarType.Neutron => 4,
            StarType.Black   => 4,
            _ => 11
        };

        // Веса типов планет по зонам (0..100). Внутри нормализуется.
        // Горячая зона: орбиты < (центр - полуширина)
        private static readonly (int type, int w)[] WeightsHot =
        {
            ((int)PlanetType.Lava,     45),
            ((int)PlanetType.Toxic,    25),
            ((int)PlanetType.Desert,   15),
            ((int)PlanetType.Stone,    10),
            ((int)PlanetType.Blasted,   5),
        };

        // Обитаемая зона: [центр - полуширина .. центр + полуширина]
        private static readonly (int type, int w)[] WeightsHab =
        {
            ((int)PlanetType.Stone,    40),
            ((int)PlanetType.Ocean,    25),
            ((int)PlanetType.Desert,   15),
            ((int)PlanetType.Toxic,    10),
            ((int)PlanetType.GasGiant,  5),
            ((int)PlanetType.IceGiant,  5),
        };

        // За снежной линией: > SnowLine
        private static readonly (int type, int w)[] WeightsIcyOuter =
        {
            ((int)PlanetType.GasGiant, 35),
            ((int)PlanetType.IceGiant, 25),
            ((int)PlanetType.Frozen,   25),
            ((int)PlanetType.Desert,    5),
            ((int)PlanetType.Stone,     5),
            ((int)PlanetType.Blasted,   5),
        };

        // Умеренно-внешняя зона (между краем «хэба» и снежной линией)
        private static readonly (int type, int w)[] WeightsWarmOuter =
        {
            ((int)PlanetType.Desert,   30),
            ((int)PlanetType.Stone,    25),
            ((int)PlanetType.Ocean,    15),
            ((int)PlanetType.Toxic,    10),
            ((int)PlanetType.GasGiant, 10),
            ((int)PlanetType.IceGiant, 10),
        };

        // Масштабы радиуса (в земных радиусах) по типам планет
        private static Vector2 RadiusRange(PlanetType p) => p switch
        {
            PlanetType.Stone     => new Vector2(0.4f,  2.5f),   // Меркурий..супер-земли
            PlanetType.Ocean     => new Vector2(0.8f,  2.5f),
            PlanetType.Desert    => new Vector2(0.6f,  2.2f),
            PlanetType.Lava      => new Vector2(0.5f,  2.0f),
            PlanetType.Toxic     => new Vector2(0.7f,  2.5f),
            PlanetType.Frozen    => new Vector2(0.5f,  2.0f),
            PlanetType.Blasted   => new Vector2(0.4f,  1.8f),

            PlanetType.GasGiant  => new Vector2(7f,   14f),     // Юпитер ~11.2 Re
            PlanetType.IceGiant  => new Vector2(3f,    6f),     // Уран/Нептун
            _ => new Vector2(1f, 1f)
        };

        // Корректировка радиуса от типа звезды (масштабирование)
        private static float StarRadiusMul(StarType t) => t switch
        {
            StarType.Blue    => 1.15f,
            StarType.White   => 1.10f,
            StarType.Yellow  => 1.00f,
            StarType.Orange  => 0.95f,
            StarType.Red     => 0.90f,
            StarType.Neutron => 0.85f,
            StarType.Black   => 0.85f,
            _ => 1f
        };

        // ========= ПУБЛИЧНО =========

        /// <summary>
        /// Создаёт планету на заданной орбите вокруг данной звезды.
        /// Назначает тип и радиус (размер) в зависимости от типа звезды и удалённости.
        /// Другие поля (атмосфера/гравитация/и т.п.) — по месту, если есть в твоём Planet.
        /// </summary>
        public static Planet Create(int orbitIndex, Star star)
        {
            // 1) Определяем «зону»
            var (center, half) = HabZone(star.type);
            int snow = SnowLine(star.type);

            (int typeInt, int w)[] weights = SelectWeights(orbitIndex, center, half, snow);

            // 2) Выбираем тип по весам (0..100)
            PlanetType pType = PickWeighted(weights);

            // 3) Определяем радиус — как «размер» планеты
            var baseR = RadiusRange(pType);
            float r = UnityEngine.Random.Range(baseR.x, baseR.y) * StarRadiusMul(star.type);

            // 4) Собираем объект
            // ВАЖНО: я не лезу в поля, которых у тебя может не быть. Ставлю только то, что точно есть в типичной модели: type и radius.
            // Если у твоего Planet другие названия — подправь ниже 2 строки.
            return new Planet
            {
                Type = pType,
                Radius = r
            };
        }

        // ========= ВНУТРЕНКА =========

        private static (int typeInt, int w)[] SelectWeights(int orbit, int center, int half, int snowLine)
        {
            if (orbit < center - half)               return WeightsHot;
            if (orbit <= center + half)              return WeightsHab;
            if (orbit > snowLine)                    return WeightsIcyOuter;
            return WeightsWarmOuter;
        }

        private static PlanetType PickWeighted((int typeInt, int w)[] items)
        {
            int total = 0;
            for (int i = 0; i < items.Length; i++) total += items[i].w;
            int r = UnityEngine.Random.Range(0, Math.Max(1, total));
            int acc = 0;
            for (int i = 0; i < items.Length; i++)
            {
                acc += items[i].w;
                if (r < acc) return (PlanetType)items[i].typeInt;
            }
            return (PlanetType)items[0].typeInt;
        }
    }
}
