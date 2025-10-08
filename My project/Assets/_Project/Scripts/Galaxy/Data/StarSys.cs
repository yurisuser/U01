using UnityEngine;

namespace _Project.Scripts.Galaxy.Data
{
    public struct StarSys
    {
        public int Id;
        public string Name;
        public Vector3 GalaxyPosition;
        public Star Star;
        public PlanetSys[] PlanetSysArr;
        public int[] PlanetOrbits;
        public int[] HyperlansIds;
        public float OldX;
        public float OldY;
    }
}