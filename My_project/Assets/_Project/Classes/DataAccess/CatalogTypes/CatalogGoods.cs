namespace _Project.DataAccess
{
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
}
