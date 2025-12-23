using System.Collections.Generic;

namespace _Project.DataAccess
{
    public static class ScannerCatalogReader
    {
        private static Dictionary<int, CatalogScanner> _cache;

        public static IReadOnlyList<CatalogScanner> GetAll()
        {
            return GameDatabaseLite.GetScanners();
        }

        public static bool TryGet(int id, out CatalogScanner scanner)
        {
            EnsureCache();
            return _cache.TryGetValue(id, out scanner);
        }

        private static void EnsureCache()
        {
            if (_cache != null)
                return;

            var list = GameDatabaseLite.GetScanners();
            var dict = new Dictionary<int, CatalogScanner>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                dict[item.Id] = item;
            }

            _cache = dict;
        }
    }
}
