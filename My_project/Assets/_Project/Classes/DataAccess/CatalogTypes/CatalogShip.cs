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
            float warpSpeed,
            float metricSpeed,
            float agility,
            float acceleration,
            float prefabSize,
            string prefabName,
            byte weaponSlots,
            int cargoSize)
        {
            Id = id;
            Key = key;
            DisplayName = displayName;
            Description = description;
            Hp = hp;
            WarpSpeed = warpSpeed;
            MetricSpeed = metricSpeed;
            Agility = agility;
            Acceleration = acceleration;
            PrefabSize = prefabSize;
            PrefabName = prefabName;
            WeaponSlots = weaponSlots;
            CargoSize = cargoSize;
        }

        public int Id { get; }
        public string Key { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public int Hp { get; }
        public float WarpSpeed { get; } // Базовая варповая скорость до применения коэффициента.
        public float MetricSpeed { get; } // Базовая метрическая скорость до применения коэффициента.
        public float Agility { get; }
        public float Acceleration { get; }
        public float PrefabSize { get; }
        public string PrefabName { get; }
        public byte WeaponSlots { get; }
        public int CargoSize { get; }
    }
}
