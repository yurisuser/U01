using System.Collections.Generic;

namespace _Project.DataAccess
{
    internal static class ConstellationNameCatalogReader
    {
        public static IReadOnlyList<CatalogConstellationName> GetAll()
        {
            return GameDatabaseLite.GetConstellationNames();
        }
    }
}
