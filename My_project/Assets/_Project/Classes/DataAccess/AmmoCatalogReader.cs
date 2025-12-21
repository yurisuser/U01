using System.Collections.Generic;

namespace _Project.DataAccess
{
    public static class AmmoCatalogReader
    {
        private static Dictionary<int, CatalogAmmo> _cache;

        public static IReadOnlyList<CatalogAmmo> GetAll()
        {
            return GameDatabaseLite.GetAmmo();
        }

        public static bool TryGet(int id, out CatalogAmmo ammo)
        {
            EnsureCache();
            return _cache.TryGetValue(id, out ammo);
        }

        private static void EnsureCache()
        {
            if (_cache != null)
                return;

            var list = GameDatabaseLite.GetAmmo();
            var dict = new Dictionary<int, CatalogAmmo>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                dict[item.Id] = item;
            }

            _cache = dict;
        }
    }
}
