using _Project.DataAccess; // для GameDatabaseLite

namespace _Project.DataAccess
{
    /// <summary>Прогревает все каталоги из базы один раз при старте.</summary>
    public static class CatalogPreloader
    {
        public static void PreloadAll(bool forceReload = false)
        {
            CATALOG.LoadAll(forceReload);
        }
    }
}
