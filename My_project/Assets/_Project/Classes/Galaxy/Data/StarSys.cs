using UnityEngine;
using _Project.Scripts.Core;
using _Project.Scripts.NPC.Fraction;
using _Project.Scripts.NPC.Individ;
using _Project.Scripts.Simulation;
using _Project.Scripts.Stations;
using _Project.DataAccess;

namespace _Project.Scripts.Galaxy.Data
{
    /// <summary>
    /// Полное описание системы: звезда, планеты, индивиды и т.д.
    /// Если система (игрок, фракция) попадает на воксель сетки,
    /// то данные берутся из SystemRegistry, иначе создаётся заново.
    /// </summary>
    public struct StarSys
    {
        public UID Uid;
        public string DisplayName;
        public int[] links;
        public int ConstellationId;
        public Fraction OwnerFrac;
        public string CustomName;
        public Vector3 GalaxyPosition;
        public Star Star;
        public PlanetSys[] PlanetSysArr;
        public Individ[] IndividArr;
        public LocalSysRuntimeContext State;
        public Station[] Stations; // станции в системе

        public bool isHome;

        public int[] PlanetOrbits;
        public float OldX;
        public float OldY;
        public float DistanceToCenter;

        public string Name => !string.IsNullOrWhiteSpace(CustomName)
            ? CustomName
            : string.IsNullOrWhiteSpace(DisplayName) ? string.Empty : DisplayName;
    }
}
