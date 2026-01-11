using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace _Project.DataAccess
{
    /// <summary>Читает фракции из файлов fraction.json в каталоге Data/Fraction.</summary>
    public static class FractionCatalogReader
    {
        private static CatalogFraction[] _all;
        private static Dictionary<int, CatalogFraction> _byId;

        public static IReadOnlyList<CatalogFraction> GetAll()
        {
            EnsureLoaded();
            return _all;
        }

        public static bool TryGet(int id, out CatalogFraction fraction)
        {
            EnsureIndex();
            return _byId.TryGetValue(id, out fraction);
        }

        private static void EnsureLoaded()
        {
            if (_all != null)
                return;

            _all = LoadFromFiles();
            _byId = null;
        }

        private static void EnsureIndex()
        {
            if (_byId != null)
                return;

            EnsureLoaded();
            var dict = new Dictionary<int, CatalogFraction>(_all.Length);
            for (int i = 0; i < _all.Length; i++)
            {
                var item = _all[i];
                dict[item.Id] = item;
            }

            _byId = dict;
        }

        private static CatalogFraction[] LoadFromFiles()
        {
            var root = Path.Combine(Application.dataPath, "_Project/Data/Fraction");
            if (!Directory.Exists(root))
                return Array.Empty<CatalogFraction>();

            var list = new List<CatalogFraction>();
            var seenIds = new HashSet<int>();

            foreach (var file in Directory.EnumerateFiles(root, "fraction.json", SearchOption.AllDirectories))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var dto = JsonUtility.FromJson<FractionFile>(json);
                    if (dto == null)
                        continue;

                    if (!seenIds.Add(dto.id))
                        continue; // дубли id игнорируем

                    list.Add(new CatalogFraction(
                        dto.id,
                        dto.name,
                        dto.bio,
                        dto.politic,
                        dto.color,
                        dto.homeSector,
                        dto.homeConstellationId,
                        dto.symbol,
                        dto.description));
                }
                catch (Exception)
                {
                    // Игнорируем битые файлы; можно логировать при необходимости.
                }
            }

            if (list.Count == 0)
                return Array.Empty<CatalogFraction>();

            return list.ToArray();
        }

        [Serializable]
        private sealed class FractionFile
        {
            public int id;
            public string name;
            public string bio;
            public string politic;
            public string color;
            public int homeSector;
            public int homeConstellationId;
            public string symbol;
            public string description;
        }
    }
}
