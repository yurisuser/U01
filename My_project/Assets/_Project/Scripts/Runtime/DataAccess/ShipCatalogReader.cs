using System;
using System.Collections.Generic;

namespace _Project.DataAccess
{
    /// <summary>
    /// Centralized access to ship catalog entries from LiteDB.
    /// Caches entries in memory and provides helper methods.
    /// </summary>
    public static class ShipCatalogReader
    {
        private static readonly Random Rng = new Random();
        private static CatalogShip[] _cache;

        public static CatalogShip GetRandomShip()
        {
            EnsureCache();
            if (_cache == null || _cache.Length == 0)
                throw new InvalidOperationException("Ship catalog database is empty or unavailable.");

            var index = Rng.Next(0, _cache.Length);
            return _cache[index];
        }

        private static void EnsureCache()
        {
            if (_cache != null)
                return;

            var list = GameDatabaseLite.GetShips();
            if (list == null || list.Count == 0)
                return;

            var arr = new List<CatalogShip>(list).ToArray();
            _cache = arr;
        }
    }
}
