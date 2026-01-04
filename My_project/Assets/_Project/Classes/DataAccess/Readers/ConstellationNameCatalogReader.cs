using System.Collections.Generic;

namespace _Project.DataAccess
{
    public static class ConstellationNameCatalogReader
    {
        private static Dictionary<int, CatalogConstellationName> _cache;

        public static IReadOnlyList<CatalogConstellationName> GetAll()
        {
            return GameDatabaseLite.GetConstellationNames();
        }

        public static bool TryGet(int id, out CatalogConstellationName name)
        {
            EnsureCache();
            return _cache.TryGetValue(id, out name);
        }

        private static void EnsureCache()
        {
            if (_cache != null)
                return;

            var list = GameDatabaseLite.GetConstellationNames();
            var dict = new Dictionary<int, CatalogConstellationName>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                dict[item.Id] = item;
            }

            _cache = dict;
        }
    }
}
