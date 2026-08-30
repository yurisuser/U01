using System;
using System.Collections.Generic;
using _Project.Industry.Recipes;

namespace _Project.DataAccess
{
    /// <summary>Единый фасад для каталогов: данные и индексы грузим один раз и читаем через CATALOG.</summary>
    public static class CATALOG
    {
        private static bool _isLoaded;

        public static IReadOnlyList<CatalogWeapon> Weapons { get; private set; } = Array.Empty<CatalogWeapon>();
        public static IReadOnlyDictionary<int, CatalogWeapon> WeaponsById { get; private set; } = EmptyIndex<CatalogWeapon>();

        public static IReadOnlyList<CatalogItem> Items { get; private set; } = Array.Empty<CatalogItem>();
        public static IReadOnlyDictionary<int, CatalogItem> ItemsById { get; private set; } = EmptyIndex<CatalogItem>();

        public static IReadOnlyList<CatalogQuest> QuestItems { get; private set; } = Array.Empty<CatalogQuest>();
        public static IReadOnlyDictionary<int, CatalogQuest> QuestItemsById { get; private set; } = EmptyIndex<CatalogQuest>();

        public static IReadOnlyList<CatalogEngine> Engines { get; private set; } = Array.Empty<CatalogEngine>();
        public static IReadOnlyDictionary<int, CatalogEngine> EnginesById { get; private set; } = EmptyIndex<CatalogEngine>();

        public static IReadOnlyList<CatalogScanner> Scanners { get; private set; } = Array.Empty<CatalogScanner>();
        public static IReadOnlyDictionary<int, CatalogScanner> ScannersById { get; private set; } = EmptyIndex<CatalogScanner>();

        public static IReadOnlyList<CatalogShield> Shields { get; private set; } = Array.Empty<CatalogShield>();
        public static IReadOnlyDictionary<int, CatalogShield> ShieldsById { get; private set; } = EmptyIndex<CatalogShield>();

        public static IReadOnlyList<CatalogShip> Ships { get; private set; } = Array.Empty<CatalogShip>();
        public static IReadOnlyDictionary<int, CatalogShip> ShipsById { get; private set; } = EmptyIndex<CatalogShip>();

        public static IReadOnlyList<CatalogFraction> Fractions { get; private set; } = Array.Empty<CatalogFraction>();
        public static IReadOnlyDictionary<int, CatalogFraction> FractionsById { get; private set; } = EmptyIndex<CatalogFraction>();

        public static IReadOnlyList<CatalogConstellationName> ConstellationNames { get; private set; } = Array.Empty<CatalogConstellationName>();
        public static IReadOnlyDictionary<int, CatalogConstellationName> ConstellationNamesById { get; private set; } = EmptyIndex<CatalogConstellationName>();

        public static IReadOnlyList<Recipe> Recipes { get; private set; } = Array.Empty<Recipe>();
        public static IReadOnlyDictionary<int, Recipe> RecipesById { get; private set; } = EmptyIndex<Recipe>();

        public static IReadOnlyList<string> StarPrefixes { get; private set; } = Array.Empty<string>();

        /// <summary>Грузит все каталоги и строит индексы. Вызывать один раз на старте.</summary>
        public static void LoadAll(bool forceReload = false, string starPrefixLanguage = "en")
        {
            if (_isLoaded && !forceReload)
                return;

            Weapons = GameDatabaseLite.GetWeapons(forceReload);
            WeaponsById = BuildIndex(Weapons, x => x.Id);

            Items = ItemCatalogReader.GetAll();
            ItemsById = BuildIndex(Items, x => x.Id);

            QuestItems = GameDatabaseLite.GetQuest(forceReload);
            QuestItemsById = BuildIndex(QuestItems, x => x.Id);

            Engines = GameDatabaseLite.GetEngines(forceReload);
            EnginesById = BuildIndex(Engines, x => x.Id);

            Scanners = GameDatabaseLite.GetScanners(forceReload);
            ScannersById = BuildIndex(Scanners, x => x.Id);

            Shields = GameDatabaseLite.GetShields(forceReload);
            ShieldsById = BuildIndex(Shields, x => x.Id);

            Ships = GameDatabaseLite.GetShips(forceReload);
            ShipsById = BuildIndex(Ships, x => x.Id);

            Fractions = FractionCatalogReader.GetAll();
            FractionsById = BuildIndex(Fractions, x => x.Id);

            ConstellationNames = ConstellationNameCatalogReader.GetAll();
            ConstellationNamesById = BuildIndex(ConstellationNames, x => x.Id);

            Recipes = RecipeCatalogReader.GetAll(forceReload);
            RecipesById = BuildIndex(Recipes, x => x.Id);

            StarPrefixes = StarPrefixCatalogReader.GetAll(starPrefixLanguage);

            _isLoaded = true;
        }

        public static void Reset()
        {
            _isLoaded = false;

            Weapons = Array.Empty<CatalogWeapon>();
            WeaponsById = EmptyIndex<CatalogWeapon>();

            Items = Array.Empty<CatalogItem>();
            ItemsById = EmptyIndex<CatalogItem>();

            QuestItems = Array.Empty<CatalogQuest>();
            QuestItemsById = EmptyIndex<CatalogQuest>();

            Engines = Array.Empty<CatalogEngine>();
            EnginesById = EmptyIndex<CatalogEngine>();

            Scanners = Array.Empty<CatalogScanner>();
            ScannersById = EmptyIndex<CatalogScanner>();

            Shields = Array.Empty<CatalogShield>();
            ShieldsById = EmptyIndex<CatalogShield>();

            Ships = Array.Empty<CatalogShip>();
            ShipsById = EmptyIndex<CatalogShip>();

            Fractions = Array.Empty<CatalogFraction>();
            FractionsById = EmptyIndex<CatalogFraction>();

            ConstellationNames = Array.Empty<CatalogConstellationName>();
            ConstellationNamesById = EmptyIndex<CatalogConstellationName>();

            Recipes = Array.Empty<Recipe>();
            RecipesById = EmptyIndex<Recipe>();

            StarPrefixes = Array.Empty<string>();
        }

        private static IReadOnlyDictionary<int, TItem> BuildIndex<TItem>(IReadOnlyList<TItem> source, Func<TItem, int> keySelector)
        {
            var dict = new Dictionary<int, TItem>(source.Count);
            for (int i = 0; i < source.Count; i++)
            {
                var item = source[i];
                dict[keySelector(item)] = item;
            }
            return dict;
        }

        private static IReadOnlyDictionary<int, TItem> EmptyIndex<TItem>() => new Dictionary<int, TItem>(0);
    }
}
