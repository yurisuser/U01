using System.Collections.Generic;

namespace _Project.DataAccess
{
    internal static class SkuCatalogReader
    {
        public static IReadOnlyList<CatalogSku> GetAll()
        {
            return GameDatabaseLite.GetSku();
        }
    }
}
