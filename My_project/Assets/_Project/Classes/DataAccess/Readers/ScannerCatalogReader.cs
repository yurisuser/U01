using System.Collections.Generic;

namespace _Project.DataAccess
{
    internal static class ScannerCatalogReader
    {
        public static IReadOnlyList<CatalogScanner> GetAll()
        {
            return GameDatabaseLite.GetScanners();
        }
    }
}
