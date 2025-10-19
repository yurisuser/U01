using System;
using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.ID;
using UnityEngine;

namespace _Project.Scripts.Galaxy.Generation
{
    public static class PlanetCreator
    {
        // ========= ТЮНИНГ (всё сверху) =========

        // «Центр» обитаемой зоны по типу звезды — в условных орбитах (1-based)
        // и «ширина» зоны (полуширина).
        private static (int center, int halfWidth) HabZone(EStarType t) => t switch
        {
            EStarType.Red     => (3, 1),   // близко к звезде
            EStarType.Orange  => (5, 1),
            EStarType.Yellow  => (7, 1),
            EStarType.White   => (9, 2),
            EStarType.Blue    => (12, 2),  // дальше
            EStarType.Neutron => (2, 0),   // условно
            EStarType.Black   => (2, 0),   // условно
            _ => (7, 1)
        };

        // «Снежная линия» — после неё растёт шанс ледяных/газовых гигантов
        private static int SnowLine(EStarType t) => t switch
        {
            EStarType.Red     => 6,
            EStarType.Orange  => 8,
            EStarType.Yellow  => 11,
            EStarType.White   => 14,
            EStarType.Blue    => 18,
            EStarType.Neutron => 4,
            EStarType.Black   => 4,
            _ => 11
        };

        // Веса типов планет по зонам (0..100). Внутри нормализуется.
        // Горячая зона: орбиты < (центр - полуширина)
        private static readonly (int type, int w)[] WeightsHot =
        {
            ((int)EPlanetType.Lava,     45),
            ((int)EPlanetType.Toxic,    25),
            ((int)EPlanetType.Desert,   15),
            ((int)EPlanetType.Stone,    10),
            ((int)EPlanetType.Blasted,   5),
        };

        // Обитаемая зона: [центр - полуширина .. центр + полуширина]
        private static readonly (int type, int w)[] WeightsHab =
        {
            ((int)EPlanetType.Stone,    40),
            ((int)EPlanetType.Ocean,    25),
            ((int)EPlanetType.Desert,   15),
            ((int)EPlanetType.Toxic,    10),
            ((int)EPlanetType.GasGiant,  5),
            ((int)EPlanetType.IceGiant,  5),
        };

        // За снежной линией: > SnowLine
        private static readonly (int type, int w)[] WeightsIcyOuter =
        {
            ((int)EPlanetType.GasGiant, 35),
            ((int)EPlanetType.IceGiant, 25),
            ((int)EPlanetType.Frozen,   25),
            ((int)EPlanetType.Desert,    5),
            ((int)EPlanetType.Stone,     5),
            ((int)EPlanetType.Blasted,   5),
        };

        // Умеренно-внешняя зона (между краем «хэба» и снежной линией)
        private static readonly (int type, int w)[] WeightsWarmOuter =
        {
            ((int)EPlanetType.Desert,   30),
            ((int)EPlanetType.Stone,    25),
            ((int)EPlanetType.Ocean,    15),
            ((int)EPlanetType.Toxic,    10),
            ((int)EPlanetType.GasGiant, 10),
            ((int)EPlanetType.IceGiant, 10),
        };

        // Масштабы радиуса (в земных радиусах) по типам планет
        private static Vector2 RadiusRange(EPlanetType p) => p switch
        {
            EPlanetType.Stone     => new Vector2(0.4f,  2.5f),   // Меркурий..супер-земли
            EPlanetType.Ocean     => new Vector2(0.8f,  2.5f),
            EPlanetType.Desert    => new Vector2(0.6f,  2.2f),
            EPlanetType.Lava      => new Vector2(0.5f,  2.0f),
            EPlanetType.Toxic     => new Vector2(0.7f,  2.5f),
            EPlanetType.Frozen    => new Vector2(0.5f,  2.0f),
            EPlanetType.Blasted   => new Vector2(0.4f,  1.8f),

            EPlanetType.GasGiant  => new Vector2(7f,   14f),     // Юпитер ~11.2 Re
            EPlanetType.IceGiant  => new Vector2(3f,    6f),     // Уран/Нептун
            _ => new Vector2(1f, 1f)
        };

        // Корректировка радиуса от типа звезды (масштабирование)
        private static float StarRadiusMul(EStarType t) => t switch
        {
            EStarType.Blue    => 1.15f,
            EStarType.White   => 1.10f,
            EStarType.Yellow  => 1.00f,
            EStarType.Orange  => 0.95f,
            EStarType.Red     => 0.90f,
            EStarType.Neutron => 0.85f,
            EStarType.Black   => 0.85f,
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
            EPlanetType pType = PickWeighted(weights);

            // 3) Определяем радиус — как «размер» планеты
            var baseR = RadiusRange(pType);
            float r = UnityEngine.Random.Range(baseR.x, baseR.y) * StarRadiusMul(star.type);

            // 4) Собираем объект
            // ВАЖНО: я не лезу в поля, которых у тебя может не быть. Ставлю только то, что точно есть в типичной модели: type и radius.
            // Если у твоего Planet другие названия — подправь ниже 2 строки.
            return new Planet
            {
                Uid = IDService.Create(EntityType.Planet),
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

        private static EPlanetType PickWeighted((int typeInt, int w)[] items)
        {
            int total = 0;
            for (int i = 0; i < items.Length; i++) total += items[i].w;
            int r = UnityEngine.Random.Range(0, Math.Max(1, total));
            int acc = 0;
            for (int i = 0; i < items.Length; i++)
            {
                acc += items[i].w;
                if (r < acc) return (EPlanetType)items[i].typeInt;
            }
            return (EPlanetType)items[0].typeInt;
        }
    }
}
