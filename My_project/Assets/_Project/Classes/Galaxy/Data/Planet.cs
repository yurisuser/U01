namespace _Project.Scripts.Galaxy.Data
{
    public struct Planet
    {
        public Core.UID Uid;
        public int NameId;         // ID записи в локализации
        public float Mass;
        public EPlanetType Type;
        public float Atmosphere;
        public float Radius;
        public float OrbitalDistance;
        public float OrbitalPeriod;
        public float Temperature;
        public float Gravity;
        public PlanetResource[] Resources;

        public string Name
        {
            get => StarNameCatalog.TryGet(NameId, out var value) ? value : string.Empty;
        }
    }
}
