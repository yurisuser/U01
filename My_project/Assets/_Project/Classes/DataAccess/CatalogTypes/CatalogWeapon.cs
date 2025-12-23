namespace _Project.DataAccess
{
    public readonly struct CatalogWeapon
    {
        public CatalogWeapon(
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
            float damage,
            float ratePerSecond,
            float range)
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
            Damage = damage;
            RatePerSecond = ratePerSecond;
            Range = range;
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
        public float Damage { get; }
        public float RatePerSecond { get; }
        public float Range { get; }
    }
}
