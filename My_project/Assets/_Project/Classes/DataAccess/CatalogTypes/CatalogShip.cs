namespace _Project.DataAccess
{
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
            byte weaponSlots,
            int cargo)
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
            Cargo = cargo;
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
        public int Cargo { get; }
    }
}
