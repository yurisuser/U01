namespace _Project.DataAccess
{
    public readonly struct CatalogItemInfo
    {
        public CatalogItemInfo(
            int id,
            string key,
            string displayName,
            string description,
            int price,
            float weight,
            bool stackable,
            int maxStack)
        {
            Id = id;
            Key = key;
            DisplayName = displayName;
            Description = description;
            Price = price;
            Weight = weight;
            Stackable = stackable;
            MaxStack = maxStack;
        }

        public int Id { get; }
        public string Key { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public int Price { get; }
        public float Weight { get; }
        public bool Stackable { get; }
        public int MaxStack { get; }
    }

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

    public readonly struct CatalogGoods
    {
        public CatalogGoods(int id, string key, string displayName, string description, int price, float weight, bool stackable, int maxStack)
        {
            Id = id;
            Key = key;
            DisplayName = displayName;
            Description = description;
            Price = price;
            Weight = weight;
            Stackable = stackable;
            MaxStack = maxStack;
        }

        public int Id { get; }
        public string Key { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public int Price { get; }
        public float Weight { get; }
        public bool Stackable { get; }
        public int MaxStack { get; }
    }

    public readonly struct CatalogAmmo
    {
        public CatalogAmmo(int id, string key, string displayName, string description, int price, float weight, bool stackable, int maxStack)
        {
            Id = id;
            Key = key;
            DisplayName = displayName;
            Description = description;
            Price = price;
            Weight = weight;
            Stackable = stackable;
            MaxStack = maxStack;
        }

        public int Id { get; }
        public string Key { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public int Price { get; }
        public float Weight { get; }
        public bool Stackable { get; }
        public int MaxStack { get; }
    }

    public readonly struct CatalogQuest
    {
        public CatalogQuest(int id, string key, string displayName, string description, int price, float weight, bool stackable, int maxStack)
        {
            Id = id;
            Key = key;
            DisplayName = displayName;
            Description = description;
            Price = price;
            Weight = weight;
            Stackable = stackable;
            MaxStack = maxStack;
        }

        public int Id { get; }
        public string Key { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public int Price { get; }
        public float Weight { get; }
        public bool Stackable { get; }
        public int MaxStack { get; }
    }

    public readonly struct CatalogEngine
    {
        public CatalogEngine(
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
            float speed)
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
            Speed = speed;
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
        public float Speed { get; }
    }

    public readonly struct CatalogScanner
    {
        public CatalogScanner(
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
            float radius)
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
    }

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

    public readonly struct CatalogShip
    {
        public CatalogShip(
            int id,
            string key,
            string displayName,
            string description,
            int hp,
            float maxSpeed,
            float agility,
            float acceleration,
            float prefabSize,
            string prefabName,
            byte weaponSlots)
        {
            Id = id;
            Key = key;
            DisplayName = displayName;
            Description = description;
            Hp = hp;
            MaxSpeed = maxSpeed;
            Agility = agility;
            Acceleration = acceleration;
            PrefabSize = prefabSize;
            PrefabName = prefabName;
            WeaponSlots = weaponSlots;
        }

        public int Id { get; }
        public string Key { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public int Hp { get; }
        public float MaxSpeed { get; }
        public float Agility { get; }
        public float Acceleration { get; }
        public float PrefabSize { get; }
        public string PrefabName { get; }
        public byte WeaponSlots { get; }
    }
}
