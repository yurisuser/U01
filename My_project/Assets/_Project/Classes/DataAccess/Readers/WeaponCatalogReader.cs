using System.Collections.Generic;

namespace _Project.DataAccess
{
    internal static class WeaponCatalogReader
    {
        public static IReadOnlyList<CatalogWeapon> GetAll()
        {
            return GameDatabaseLite.GetWeapons();
        }
    }
}
