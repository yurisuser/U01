namespace _Project.DataAccess
{
    public readonly struct WeaponRow
    {
        public WeaponRow(int id, string key, string displayName, string description, float damage, float ratePerSecond, float range)
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
}
