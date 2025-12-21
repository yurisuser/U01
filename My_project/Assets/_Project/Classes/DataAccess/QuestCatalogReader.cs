using System.Collections.Generic;

namespace _Project.DataAccess
{
    public static class QuestCatalogReader
    {
        private static Dictionary<int, CatalogQuest> _cache;

        public static IReadOnlyList<CatalogQuest> GetAll()
        {
            return GameDatabaseLite.GetQuest();
        }

        public static bool TryGet(int id, out CatalogQuest quest)
        {
            EnsureCache();
            return _cache.TryGetValue(id, out quest);
        }

        private static void EnsureCache()
        {
            if (_cache != null)
                return;

            var list = GameDatabaseLite.GetQuest();
            var dict = new Dictionary<int, CatalogQuest>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                dict[item.Id] = item;
            }

            _cache = dict;
        }
    }
}
