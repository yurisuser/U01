using UnityEngine;
using _Project.Scripts.Core;
using _Project.Scripts.NPC.Fraction;
using _Project.Scripts.NPC.Individ;
using _Project.Scripts.Simulation;
using _Project.Scripts.Stations;

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
        public int NameId;
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

        public int[] PlanetOrbits;
        public float OldX;
        public float OldY;
        public float DistanceToCenter;

        public string Name
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(CustomName))
                    return CustomName;

                if (NameId < 0)
                    return string.Empty;

                return LocalizationDatabase.TryGetStarName(NameId, OldX, OldY, out var value)
                    ? value
                    : string.Empty;
            }
        }
    }
}
