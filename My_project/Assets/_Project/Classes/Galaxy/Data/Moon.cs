using _Project.Scripts.Core;

namespace _Project.Scripts.Galaxy.Data
{
    public struct Moon
    {
        public Core.UID Uid;
        public string Name;
        public EMoonType Type;
        public EMoonSize Size;
        public int OrbitIndex;
        public float Mass;
        public float Radius;
        public float OrbitDistance;
        public float OrbitPeriod;
        public float Inclination;
        public float Atmosphere;
        public float Temperature;
        public float Gravity;
        public bool isHome;
        public ResourceDeposit[] ResourceDeposits;

        public string DisplayName => string.IsNullOrWhiteSpace(Name) ? string.Empty : Name;
    }
}
