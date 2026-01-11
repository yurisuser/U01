using System.Collections.Generic;

namespace _Project.DataAccess
{
    /// <summary>
    /// Каркас каталога: публичные коллекции данных, загружаемых извне (БД/файлы).
    /// Заполнение и индексация будут добавлены позже.
    /// </summary>
    public static class CATALOG
    {
        public static IReadOnlyList<CatalogWeapon> Weapons;
        public static IReadOnlyDictionary<int, CatalogWeapon> WeaponsById;

        public static IReadOnlyList<CatalogGoods> Goods;
        public static IReadOnlyDictionary<int, CatalogGoods> GoodsById;

        public static IReadOnlyList<CatalogQuest> QuestItems;
        public static IReadOnlyDictionary<int, CatalogQuest> QuestItemsById;

        public static IReadOnlyList<CatalogEngine> Engines;
        public static IReadOnlyDictionary<int, CatalogEngine> EnginesById;

        public static IReadOnlyList<CatalogScanner> Scanners;
        public static IReadOnlyDictionary<int, CatalogScanner> ScannersById;

        public static IReadOnlyList<CatalogShield> Shields;
        public static IReadOnlyDictionary<int, CatalogShield> ShieldsById;

        public static IReadOnlyList<CatalogShip> Ships;
        public static IReadOnlyDictionary<int, CatalogShip> ShipsById;

        public static IReadOnlyList<CatalogFraction> Fractions;
        public static IReadOnlyDictionary<int, CatalogFraction> FractionsById;

        public static IReadOnlyList<CatalogConstellationName> ConstellationNames;
        public static IReadOnlyDictionary<int, CatalogConstellationName> ConstellationNamesById;
    }
}
