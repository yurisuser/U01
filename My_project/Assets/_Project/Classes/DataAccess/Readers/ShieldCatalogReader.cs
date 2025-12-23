using System.Collections.Generic;

namespace _Project.DataAccess
{
    public static class ShieldCatalogReader
    {
        private static Dictionary<int, CatalogShield> _cache;

        public static IReadOnlyList<CatalogShield> GetAll()
        {
            return GameDatabaseLite.GetShields();
        }

        public static bool TryGet(int id, out CatalogShield shield)
        {
            EnsureCache();
            return _cache.TryGetValue(id, out shield);
        }

        private static void EnsureCache()
        {
            if (_cache != null)
                return;

            var list = GameDatabaseLite.GetShields();
            var dict = new Dictionary<int, CatalogShield>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                dict[item.Id] = item;
            }

            _cache = dict;
        }
    }
}
