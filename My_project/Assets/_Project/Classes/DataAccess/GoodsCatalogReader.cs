using System.Collections.Generic;

namespace _Project.DataAccess
{
    public static class GoodsCatalogReader
    {
        private static Dictionary<int, CatalogGoods> _cache;

        public static IReadOnlyList<CatalogGoods> GetAll()
        {
            return GameDatabaseLite.GetGoods();
        }

        public static bool TryGet(int id, out CatalogGoods goods)
        {
            EnsureCache();
            return _cache.TryGetValue(id, out goods);
        }

        private static void EnsureCache()
        {
            if (_cache != null)
                return;

            var list = GameDatabaseLite.GetGoods();
            var dict = new Dictionary<int, CatalogGoods>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                dict[item.Id] = item;
            }

            _cache = dict;
        }
    }
}
