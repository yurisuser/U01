using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace _Project.DataAccess
{
    /// <summary>Читает фракции из файлов fraction.json в каталоге Data/Fraction.</summary>
    internal static class FractionCatalogReader
    {
        public static IReadOnlyList<CatalogFraction> GetAll()
        {
            return LoadFromFiles();
        }

        private static CatalogFraction[] LoadFromFiles()
        {
            var root = Path.Combine(Application.dataPath, "_Project/Data/Fraction");
            if (!Directory.Exists(root))
                throw new DirectoryNotFoundException($"Каталог фракций не найден: {root}");

            var list = new List<CatalogFraction>();
            var seenIds = new HashSet<int>();

            foreach (var file in Directory.EnumerateFiles(root, "fraction.json", SearchOption.AllDirectories))
            {
                var json = File.ReadAllText(file);
                var dto = JsonUtility.FromJson<FractionFile>(json);
                if (dto == null)
                    throw new InvalidOperationException($"Не удалось распарсить fraction.json: {file}");

                if (!seenIds.Add(dto.id))
                    throw new InvalidOperationException($"Дублирующийся id фракции {dto.id} в файле {file}");

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

            if (list.Count == 0)
                throw new InvalidOperationException("Не найдено ни одного fraction.json");

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
