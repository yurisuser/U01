using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using _Project.Scripts.NPC.Fraction;

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

                var dir = Path.GetDirectoryName(file);
                var starNames = ReadNames(dir, "stars");
                var planetNames = ReadNames(dir, "planet");
                var moonNames = ReadNames(dir, "moon");
                var fractionType = ParseFractionType(dto.fractionType, file);

                list.Add(new CatalogFraction(
                    dto.id,
                    dto.name,
                    dto.bio,
                    dto.politic,
                    dto.color,
                    dto.homeSector,
                    dto.homeConstellationId,
                    fractionType,
                    dto.symbol,
                    dto.description,
                    starNames,
                    planetNames,
                    moonNames,
                    dir));
            }

            if (list.Count == 0)
                throw new InvalidOperationException("Не найдено ни одного fraction.json");

            return list.ToArray();
        }

        private static IReadOnlyList<string> ReadNames(string dir, string kind)
        {
            if (string.IsNullOrWhiteSpace(dir))
                return Array.Empty<string>();

            var fileName = $"names.en.{kind}.json";
            var path = Path.Combine(dir, fileName);
            if (!File.Exists(path))
                return Array.Empty<string>();

            var raw = File.ReadAllText(path);
            var wrapped = $"{{\"items\":{raw}}}";
            var dto = JsonUtility.FromJson<NameList>(wrapped);
            if (dto?.items == null || dto.items.Count == 0)
                return Array.Empty<string>();

            return dto.items.ToArray();
        }

        private static EFractionTypes ParseFractionType(string raw, string file)
        {
            if (!string.IsNullOrWhiteSpace(raw)
                && Enum.TryParse<EFractionTypes>(raw, ignoreCase: true, out var parsed))
                return parsed;

            Debug.LogWarning($"fractionType не задан или не распознан в {file}, используется Regular");
            return EFractionTypes.Regular;
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
            public string fractionType;
            public string symbol;
            public string description;
        }

        [Serializable]
        private sealed class NameList
        {
            public List<string> items;
        }
    }
}
