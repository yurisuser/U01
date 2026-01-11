using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;

public sealed class LocalizationReader
{
    private static readonly Regex RangePattern = new(@"^[a-z]{2}-(\d+)(?:-|_)(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    /// <summary>
    /// Reads all English localization files from the provided directory.
    /// </summary>
    /// <param name="directoryPath">Directory that contains files like en-0-1000.json.</param>
    public IEnumerable<LocalizationChunk> ReadEnglishChunks(string directoryPath)
    {
        if (directoryPath == null)
            throw new ArgumentNullException(nameof(directoryPath));

        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Localization directory not found: {directoryPath}");

        foreach (var file in Directory.EnumerateFiles(directoryPath, "en-*.json", SearchOption.TopDirectoryOnly))
        {
            yield return ReadChunk(file);
        }
    }

    private static LocalizationChunk ReadChunk(string filePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var match = RangePattern.Match(fileName ?? string.Empty);
        int? declaredStartId = null;
        int? declaredEndId = null;
        if (match.Success && match.Groups.Count >= 3)
        {
            declaredStartId = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            declaredEndId = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            if (declaredEndId < declaredStartId)
                throw new InvalidOperationException($"Invalid range in file name \"{fileName}\": end < start.");
        }

        var serializer = new DataContractJsonSerializer(typeof(List<LocalizationEntry>));
        List<LocalizationEntry> entries;
        using (var stream = File.OpenRead(filePath))
        {
            entries = serializer.ReadObject(stream) as List<LocalizationEntry> ?? new List<LocalizationEntry>();
        }

        if (entries.Count == 0)
        {
            if (declaredStartId.HasValue && declaredEndId.HasValue)
                return new LocalizationChunk(declaredStartId.Value, declaredEndId.Value);

            throw new InvalidOperationException($"File \"{fileName}\" does not contain localization entries and its name does not declare an id range.");
        }

        int minId = int.MaxValue;
        int maxId = int.MinValue;
        foreach (var entry in entries)
        {
            if (entry == null)
                continue;

            if (entry.Id < minId)
                minId = entry.Id;

            if (entry.Id > maxId)
                maxId = entry.Id;
        }

        if (minId == int.MaxValue || maxId == int.MinValue)
            throw new InvalidOperationException($"File \"{fileName}\" does not contain valid localization entries.");

        int chunkStartId = minId;
        int chunkEndId = maxId;

        var chunk = new LocalizationChunk(chunkStartId, chunkEndId);
        foreach (var entry in entries)
        {
            if (entry == null)
                continue;

            if (!chunk.TryAdd(entry.Id, entry.Value ?? string.Empty, out var error))
                throw new InvalidOperationException($"Error in file \"{fileName}\": {error}");
        }

        return chunk;
    }

    [DataContract]
    private sealed class LocalizationEntry
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "value")]
        public string Value { get; set; }
    }
}
