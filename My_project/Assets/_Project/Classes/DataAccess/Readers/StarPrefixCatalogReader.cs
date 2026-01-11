using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace _Project.DataAccess
{
    /// <summary>Читает префиксы имён звёзд из файлов names.&lt;lang&gt;.star_prefix.json.</summary>
    public static class StarPrefixCatalogReader
    {
        private static string[] _prefixes;

        public static IReadOnlyList<string> GetAll(string languageCode = "en")
        {
            if (_prefixes != null)
                return _prefixes;

            _prefixes = Load(languageCode);
            return _prefixes;
        }

        private static string[] Load(string languageCode)
        {
            var root = Path.Combine(Application.dataPath, "_Project/Data/Localization");
            var fileName = $"names.{languageCode}.star_prefix.json";
            var path = Path.Combine(root, fileName);

            if (!File.Exists(path))
                throw new FileNotFoundException($"Не найден файл префиксов звёзд: {path}");

            try
            {
                var raw = File.ReadAllText(path);
                var wrapped = $"{{\"items\":{raw}}}";
                var dto = JsonUtility.FromJson<PrefixFile>(wrapped);
                if (dto?.items == null || dto.items.Count == 0)
                    throw new InvalidOperationException($"Файл \"{path}\" пуст или содержит неверные данные.");

                return dto.items.ToArray();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при чтении префиксов звёзд из {path}: {ex.Message}", ex);
            }
        }

        [Serializable]
        private sealed class PrefixFile
        {
            public List<string> items;
        }
    }
}
