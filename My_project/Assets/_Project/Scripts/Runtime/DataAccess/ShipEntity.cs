namespace _Project.DataAccess
{
    public readonly struct ShipEntity
    {
        public ShipEntity(int id, string key, string displayName, string description, int hp, float maxSpeed, float agility, byte weaponSlots)
        {
            Id = id;
            Key = key;
            DisplayName = displayName;
            Description = description;
            Hp = hp;
            MaxSpeed = maxSpeed;
            Agility = agility;
            WeaponSlots = weaponSlots;
        }

        public int Id { get; }
        public string Key { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public int Hp { get; }
        public float MaxSpeed { get; }
        public float Agility { get; }
        public byte WeaponSlots { get; }
    }
}
