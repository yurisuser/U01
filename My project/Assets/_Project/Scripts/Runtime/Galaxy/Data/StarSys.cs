using UnityEngine;
using _Project.Scripts.Core;
using _Project.Scripts.NPC.Individ;
using _Project.Scripts.Ships;

namespace _Project.Scripts.Galaxy.Data
{
    public struct StarSys
    {
        public UID Uid;
        public string Name;
        public Vector3 GalaxyPosition;
        public Star Star;
        public PlanetSys[] PlanetSysArr;
        public Ship[] ShipArr;
        public Individ[] IndividArr;
        
        public int[] PlanetOrbits;
        public float OldX;
        public float OldY;
    }
}