namespace _Project.DataAccess
{
    /// <summary>Единица каталога ресурсов/товаров (SKU).</summary>
    public readonly struct CatalogSku
    {
        public CatalogSku(int id, string name, string description, string img, float price, bool isMineable, bool isIndustrial, bool isConsumable, bool isLootOnly, float peakOrbit, float orbitSpread, float metallicityFactor, float peakOrbitNorm, float orbitSpreadNorm)
        {
            Id = id;
            Name = name;
            Description = description;
            Img = img;
            Price = price;
            IsMineable = isMineable;
            IsIndustrial = isIndustrial;
            IsConsumable = isConsumable;
            IsLootOnly = isLootOnly;
            PeakOrbit = peakOrbit;
            OrbitSpread = orbitSpread;
            MetallicityFactor = metallicityFactor;
            PeakOrbitNorm = peakOrbitNorm;
            OrbitSpreadNorm = orbitSpreadNorm;
        }

        public int Id { get; }
        public string Name { get; }
        public string Description { get; }
        public string Img { get; }
        public float Price { get; }
        public bool IsMineable { get; }
        public bool IsIndustrial { get; }
        public bool IsConsumable { get; }
        public bool IsLootOnly { get; }
        public float PeakOrbit { get; }
        public float OrbitSpread { get; }
        public float MetallicityFactor { get; }
        public float PeakOrbitNorm { get; }
        public float OrbitSpreadNorm { get; }
    }
}
