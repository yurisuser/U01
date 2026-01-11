using System.Collections.Generic;
using _Project.DataAccess;

namespace _Project.Scripts.Galaxy.Constellations
{
    public static class ConstellationService
    {
        private static CatalogConstellationName[] _all;
        private static Dictionary<int, CatalogConstellationName> _byId;

        public static IReadOnlyList<CatalogConstellationName> GetAll() => _all ??= Load();

        public static void ReloadAll()
        {
            _all = Load();
            _byId = null;
        }

        public static string GetNameById(int id)
        {
            if (id <= 0)
                return "Unknow";

            var text = GetTextByIdInternal(id);
            if (!string.IsNullOrWhiteSpace(text))
                return text;

            return "Unknow";
        }

        private static string GetTextByIdInternal(int id)
        {
            EnsureIndex();
            return _byId != null && _byId.TryGetValue(id, out var name) ? name.Text : null;
        }

        private static CatalogConstellationName[] Load()
        {
            var list = CATALOG.ConstellationNames;
            if (list == null || list.Count == 0)
                return new[] { new CatalogConstellationName(0, string.Empty) };

            var result = new CatalogConstellationName[list.Count];
            for (int i = 0; i < list.Count; i++)
                result[i] = list[i];

            return result;
        }

        private static void EnsureIndex()
        {
            if (_byId != null)
                return;

            var all = GetAll();
            var dict = new Dictionary<int, CatalogConstellationName>(all.Count);
            for (int i = 0; i < all.Count; i++)
                dict[all[i].Id] = all[i];

            _byId = dict;
        }
    }
}
