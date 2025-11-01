using UnityEngine;
using _Project.Scripts.Core;
using _Project.Scripts.NPC.Individ;

namespace _Project.Scripts.Galaxy.Data
{
    /// <summary>
    /// Статичное описание системы: звезда, планеты, вспомогательные данные.
    /// Живые объекты (корабли, события) отныне не храним здесь,
    /// их состояние берём из SystemRegistry, когда нужно показать систему.
    /// </summary>
    public struct StarSys
    {
        public UID Uid;
        public string Name;
        public Vector3 GalaxyPosition;
        public Star Star;
        public PlanetSys[] PlanetSysArr;
        public Individ[] IndividArr;
        
        public int[] PlanetOrbits;
        public float OldX;
        public float OldY;
    }
}
