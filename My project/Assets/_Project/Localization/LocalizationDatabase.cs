using System;
using System.Collections.Generic;

public static class LocalizationDatabase
{
    private static readonly List<LocalizationChunk> _chunks = new();
    private static bool _isInitialized;

    public static IReadOnlyList<LocalizationChunk> Chunks => _chunks;
    public static bool IsInitialized => _isInitialized;

    /// <summary>
    /// Loads all English localization chunks from the specified directory.
    /// </summary>
    public static void Initialize(string directoryPath)
    {
        var reader = new LocalizationReader();

        _chunks.Clear();
        foreach (var chunk in reader.ReadEnglishChunks(directoryPath))
        {
            _chunks.Add(chunk);
        }

        _chunks.Sort((a, b) => a.StartId.CompareTo(b.StartId));
        _isInitialized = true;
    }

    public static bool TryGet(int id, out string value)
    {
        EnsureInitialized();
        foreach (var chunk in _chunks)
        {
            if (!chunk.Contains(id))
                continue;

            if (chunk.TryGetValue(id, out value))
                return true;

            break;
        }

        value = string.Empty;
        return false;
    }

    public static string Get(int id)
    {
        if (TryGet(id, out var value))
            return value;

        throw new KeyNotFoundException($"Localization id {id} was not loaded.");
    }

    private static void EnsureInitialized()
    {
        if (!_isInitialized)
            throw new InvalidOperationException("LocalizationDatabase.Initialize must be called before accessing data.");
    }
}

public sealed class LocalizationChunk
{
    private readonly string[] _values;

    public LocalizationChunk(int startId, int endId)
    {
        if (endId < startId)
            throw new ArgumentOutOfRangeException(nameof(endId), "End id must be greater or equal to start id.");

        StartId = startId;
        EndId = endId;
        _values = new string[endId - startId + 1];
    }

    public int StartId { get; }
    public int EndId { get; }
    public IReadOnlyList<string> Values => _values;

    public bool Contains(int id) => id >= StartId && id <= EndId;

    public bool TryAdd(int id, string value, out string error)
    {
        if (!Contains(id))
        {
            error = $"Id {id} is outside of chunk range [{StartId}, {EndId}].";
            return false;
        }

        var index = id - StartId;
        if (_values[index] != null)
        {
            error = $"Duplicate entry for id {id}.";
            return false;
        }

        _values[index] = value;
        error = string.Empty;
        return true;
    }

    public bool TryGetValue(int id, out string value)
    {
        if (!Contains(id))
        {
            value = string.Empty;
            return false;
        }

        var stored = _values[id - StartId];
        if (stored == null)
        {
            value = string.Empty;
            return false;
        }

        value = stored;
        return true;
    }

    public string this[int id] => _values[id - StartId] ?? string.Empty;
}
