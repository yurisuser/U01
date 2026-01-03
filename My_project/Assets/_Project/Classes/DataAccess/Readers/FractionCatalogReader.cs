using System.Collections.Generic;

namespace _Project.DataAccess
{
    public static class FractionCatalogReader
    {
        private static Dictionary<int, CatalogFraction> _cache;

        public static IReadOnlyList<CatalogFraction> GetAll()
        {
            return GameDatabaseLite.GetFractions();
        }

        public static bool TryGet(int id, out CatalogFraction fraction)
        {
            EnsureCache();
            return _cache.TryGetValue(id, out fraction);
        }

        private static void EnsureCache()
        {
            if (_cache != null)
                return;

            var list = GameDatabaseLite.GetFractions();
            var dict = new Dictionary<int, CatalogFraction>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                dict[item.Id] = item;
            }

            _cache = dict;
        }
    }
}
