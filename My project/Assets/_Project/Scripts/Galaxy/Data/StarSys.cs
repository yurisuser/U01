using _Project.Scripts.ID;
using UnityEngine;

namespace _Project.Scripts.Galaxy.Data
{
    public struct StarSys
    {
        public UID UID;
        public string Name;
        public Vector3 GalaxyPosition;
        public Star Star;
        public PlanetSys[] PlanetSysArr;
        public int[] PlanetOrbits;
        public float OldX;
        public float OldY;
    }
}