namespace _Project.DataAccess
{
    public readonly struct CatalogWeapon
    {
        public CatalogWeapon(int id, string key, string displayName, string description, float damage, float ratePerSecond, float range)
        {
            Id = id;
            Key = key;
            DisplayName = displayName;
            Description = description;
            Damage = damage;
            RatePerSecond = ratePerSecond;
            Range = range;
        }

        public int Id { get; }
        public string Key { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public float Damage { get; }
        public float RatePerSecond { get; }
        public float Range { get; }
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
