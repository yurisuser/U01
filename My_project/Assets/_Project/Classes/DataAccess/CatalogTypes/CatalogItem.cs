namespace _Project.DataAccess
{
    /// <summary>Единица каталога предметов (items).</summary>
    public readonly struct CatalogItem
    {
        public CatalogItem(
            int id,
            string key,
            string name,
            string description,
            string img,
            float price,
            bool isMineable,
            bool isIndustrial,
            bool isConsumable,
            bool isLootOnly,
            float peakOrbit,
            float orbitSpread,
            float metallicityFactor,
            float peakOrbitNorm,
            float orbitSpreadNorm,
            float weight,
            bool stackable,
            int maxStack)
        {
            Id = id;
            Key = key;
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
            Weight = weight;
            Stackable = stackable;
            MaxStack = maxStack;
        }

        public int Id { get; }
        public string Key { get; } // текстовый ключ предмета
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
        public float Weight { get; }
        public bool Stackable { get; }
        public int MaxStack { get; }
    }
}
