using System.Collections.Generic;

namespace _Project.DataAccess
{
    public static class WeaponCatalogReader
    {
        private static Dictionary<int, CatalogWeapon> _cache;

        public static IReadOnlyList<CatalogWeapon> GetAll()
        {
            return GameDatabaseLite.GetWeapons();
        }

        public static bool TryGet(int id, out CatalogWeapon weapon)
        {
            EnsureCache();
            return _cache.TryGetValue(id, out weapon);
        }

        private static void EnsureCache()
        {
            if (_cache != null)
                return;

            var list = GameDatabaseLite.GetWeapons();
            var dict = new Dictionary<int, CatalogWeapon>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                dict[item.Id] = item;
            }

            _cache = dict;
        }
    }
}
