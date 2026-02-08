using System;
using System.Collections.Generic;

namespace _Project.DataAccess
{
    /// <summary>
    /// Centralized access to ship catalog entries from SQLite.
    /// </summary>
    internal static class ShipCatalogReader
    {
        private static readonly Random Rng = new Random();
        private static readonly object RngSync = new object();
        private static CatalogShip[] _cache;

        public static IReadOnlyList<CatalogShip> GetAll()
        {
            if (_cache == null)
            {
                var list = GameDatabaseLite.GetShips();
                _cache = list == null || list.Count == 0
                    ? Array.Empty<CatalogShip>()
                    : new List<CatalogShip>(list).ToArray();
            }
            return _cache;
        }

        public static CatalogShip GetRandomShip()
        {
            var all = GetAll();
            if (all == null || all.Count == 0)
                throw new InvalidOperationException("Ship catalog database is empty or unavailable.");

            int index;
            lock (RngSync)
            {
                index = Rng.Next(0, all.Count);
            }
            return all[index];
        }
    }
}
