namespace _Project.Industry.Recipes
{
    /// <summary>Технологический тип преобразования предметов.</summary>
    public enum ERecipeType
    {
        None = 0,                  // тип рецепта не задан
        Extraction = 1,            // добыча сырья
        Enrichment = 2,            // обогащение сырья
        Smelting = 3,              // плавка
        Chemistry = 4,             // химическое преобразование
        NuclearTransformation = 5, // ядерное преобразование
        Manufacturing = 6,         // промышленное производство
        HighTechProduction = 7,    // высокотехнологичное производство
        Assembly = 8,              // сборка готовых изделий
        Recycling = 9              // переработка отходов и изделий
    }
}
