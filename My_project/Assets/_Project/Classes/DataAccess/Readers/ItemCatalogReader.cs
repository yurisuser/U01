using System.Collections.Generic;

namespace _Project.DataAccess
{
    internal static class ItemCatalogReader
    {
        public static IReadOnlyList<CatalogItem> GetAll()
        {
            return GameDatabaseLite.GetItems();
        }
    }
}
