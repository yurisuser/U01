using System.Collections.Generic;

namespace _Project.DataAccess
{
    internal static class EngineCatalogReader
    {
        public static IReadOnlyList<CatalogEngine> GetAll()
        {
            return GameDatabaseLite.GetEngines();
        }
    }
}
