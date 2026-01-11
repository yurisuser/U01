using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

public sealed class LocalizationReader
{
    /// <summary>Читает префиксы имён звёзд из файла вида names.&lt;lang&gt;.star_prefix.json.</summary>
    public IReadOnlyList<string> ReadStarPrefixes(string directoryPath, string languageCode = "en")
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentNullException(nameof(directoryPath));

        var fileName = $"names.{languageCode}.star_prefix.json";
        var filePath = Path.Combine(directoryPath, fileName);
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Не найден файл локализации префиксов звёзд: {filePath}");

        List<string> prefixes;
        var serializer = new DataContractJsonSerializer(typeof(List<string>));
        using (var stream = File.OpenRead(filePath))
        {
            prefixes = serializer.ReadObject(stream) as List<string> ?? new List<string>();
        }

        if (prefixes.Count == 0)
            throw new InvalidOperationException($"Файл \"{fileName}\" не содержит префиксов имён звёзд.");

        return prefixes;
    }
}
