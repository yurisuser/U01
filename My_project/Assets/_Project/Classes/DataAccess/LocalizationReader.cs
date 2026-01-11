using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

public sealed class LocalizationReader
{
    /// <summary>Читает список префиксов имён звёзд из файла вида names.&lt;lang&gt;.star_prefix.json.</summary>
    public LocalizationChunk ReadStarPrefixChunk(string directoryPath, string languageCode = "en")
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

        var chunk = new LocalizationChunk(0, prefixes.Count - 1);
        for (int i = 0; i < prefixes.Count; i++)
        {
            var value = prefixes[i] ?? string.Empty;
            if (!chunk.TryAdd(i, value, out var error))
                throw new InvalidOperationException($"Ошибка в файле \"{fileName}\": {error}");
        }

        return chunk;
    }
}
