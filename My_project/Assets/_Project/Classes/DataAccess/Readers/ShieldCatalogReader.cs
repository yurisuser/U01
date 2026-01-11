using System.Collections.Generic;

namespace _Project.DataAccess
{
    internal static class ShieldCatalogReader
    {
        public static IReadOnlyList<CatalogShield> GetAll()
        {
            return GameDatabaseLite.GetShields();
        }
    }
}
