using System.Collections.Generic;

namespace _Project.DataAccess
{
    internal static class QuestCatalogReader
    {
        public static IReadOnlyList<CatalogQuest> GetAll()
        {
            return GameDatabaseLite.GetQuest();
        }
    }
}
