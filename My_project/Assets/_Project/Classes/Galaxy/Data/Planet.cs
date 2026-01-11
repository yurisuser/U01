namespace _Project.Scripts.Galaxy.Data
{
    public struct Planet
    {
        public Core.UID Uid;
        public string Name;
        public float Mass;
        public EPlanetType Type;
        public float Atmosphere;
        public float Radius;
        public float OrbitalDistance;
        public float OrbitalPeriod;
        public float Temperature;
        public float Gravity;
        public PlanetResource[] Resources;

        public string DisplayName => string.IsNullOrWhiteSpace(Name) ? string.Empty : Name;
    }
}
