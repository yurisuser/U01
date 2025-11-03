using UnityEngine;
using _Project.Scripts.Core;
using _Project.Scripts.NPC.Individ;

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
        public Vector3 GalaxyPosition;
        public Star Star;
        public PlanetSys[] PlanetSysArr;
        public Individ[] IndividArr;

        public int[] PlanetOrbits;
        public float OldX;
        public float OldY;

        public string Name
        {
            get
            {
                if (NameId < 0)
                    return string.Empty;

                return LocalizationDatabase.TryGetStarName(NameId, OldX, OldY, out var value)
                    ? value
                    : string.Empty;
            }
        }
    }
}
