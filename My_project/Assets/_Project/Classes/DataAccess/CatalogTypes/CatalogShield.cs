namespace _Project.DataAccess
{
    public readonly struct CatalogShield
    {
        public CatalogShield(
            int id,
            string key,
            string displayName,
            string description,
            int price,
            float weight,
            bool stackable,
            int maxStack,
            int techLevel,
            float powerUse,
            float cpuUse,
            float radius,
            float volume,
            float regen)
        {
            Id = id;
            Key = key;
            DisplayName = displayName;
            Description = description;
            Price = price;
            Weight = weight;
            Stackable = stackable;
            MaxStack = maxStack;
            TechLevel = techLevel;
            PowerUse = powerUse;
            CpuUse = cpuUse;
            Radius = radius;
            Volume = volume;
            Regen = regen;
        }

        public int Id { get; }
        public string Key { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public int Price { get; }
        public float Weight { get; }
        public bool Stackable { get; }
        public int MaxStack { get; }
        public int TechLevel { get; }
        public float PowerUse { get; }
        public float CpuUse { get; }
        public float Radius { get; }
        public float Volume { get; }
        public float Regen { get; }
    }
}
