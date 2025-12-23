using System.Collections.Generic;

namespace _Project.DataAccess
{
    public static class EngineCatalogReader
    {
        private static Dictionary<int, CatalogEngine> _cache;

        public static IReadOnlyList<CatalogEngine> GetAll()
        {
            return GameDatabaseLite.GetEngines();
        }

        public static bool TryGet(int id, out CatalogEngine engine)
        {
            EnsureCache();
            return _cache.TryGetValue(id, out engine);
        }

        private static void EnsureCache()
        {
            if (_cache != null)
                return;

            var list = GameDatabaseLite.GetEngines();
            var dict = new Dictionary<int, CatalogEngine>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                dict[item.Id] = item;
            }

            _cache = dict;
        }
    }
}
