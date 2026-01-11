using System.Collections.Generic;

namespace _Project.DataAccess
{
    internal static class GoodsCatalogReader
    {
        public static IReadOnlyList<CatalogGoods> GetAll()
        {
            return GameDatabaseLite.GetGoods();
        }
    }
}
