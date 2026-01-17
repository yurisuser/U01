namespace _Project.DataAccess
{
    /// <summary>Единица каталога ресурсов/товаров (SKU).</summary>
    public readonly struct CatalogSku
    {
        public CatalogSku(int id, string name, string description, string img, float price, bool isMineable, bool isIndustrial, bool isConsumable)
        {
            Id = id;
            Name = name;
            Description = description;
            Img = img;
            Price = price;
            IsMineable = isMineable;
            IsIndustrial = isIndustrial;
            IsConsumable = isConsumable;
        }

        public int Id { get; }
        public string Name { get; }
        public string Description { get; }
        public string Img { get; }
        public float Price { get; }
        public bool IsMineable { get; }
        public bool IsIndustrial { get; }
        public bool IsConsumable { get; }
    }
}
