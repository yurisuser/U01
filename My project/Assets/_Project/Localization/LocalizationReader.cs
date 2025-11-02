using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;

public sealed class LocalizationReader
{
    private static readonly Regex RangePattern = new(@"^[a-z]{2}-(\d+)-(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
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
        if (!match.Success || match.Groups.Count < 3)
            throw new InvalidOperationException($"File name \"{fileName}\" does not match expected pattern en-<start>-<end>.json");

        int startId = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        int endId = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
        if (endId < startId)
            throw new InvalidOperationException($"Invalid range in file name \"{fileName}\": end < start.");

        var serializer = new DataContractJsonSerializer(typeof(List<LocalizationEntry>));
        List<LocalizationEntry> entries;
        using (var stream = File.OpenRead(filePath))
        {
            entries = serializer.ReadObject(stream) as List<LocalizationEntry> ?? new List<LocalizationEntry>();
        }

        var chunk = new LocalizationChunk(startId, endId);
        foreach (var entry in entries)
        {
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
