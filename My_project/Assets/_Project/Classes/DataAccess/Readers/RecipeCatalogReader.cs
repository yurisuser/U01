using System.Collections.Generic;
using _Project.Industry.Recipes;

namespace _Project.DataAccess
{
    /// <summary>Читает каталог производственных рецептов из SQLite.</summary>
    public static class RecipeCatalogReader
    {
        public static IReadOnlyList<Recipe> GetAll(bool forceReload = false)
        {
            return GameDatabaseLite.GetRecipes(forceReload);
        }
    }
}
