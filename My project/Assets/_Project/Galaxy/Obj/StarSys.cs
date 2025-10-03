using UnityEngine;

namespace _Project.Galaxy.Obj
{
    public struct StarSys
    {
        public int Id;
        public string Name;
        public Vector3 GalaxyPosition;
        public Star Star;
        public PlanetSys[] PlanetSysArr;
        public int[] HyperlansIds;
        public float OldX;
        public float OldY;
    }
}