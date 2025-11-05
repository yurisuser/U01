using System;
using System.Collections.Generic;
using System.Globalization;

public static class LocalizationDatabase
{
    // каждый диапазон — свой файл и массив
    // 0-49 - префикс имени звезды (IRAS-3587, HIP-0749 etc)
    // 51-100 - латинские названия звезд (Fraction 1)
    // 101-150 - ацтекские названия звезд (Fraction 2)
    // 151-200 - скандинавские названия звезд (Fraction 3)
    // 201-250 - греческие названия звезд (Fraction 4)
    // 251-300 - машинные названия звезд (Fraction 5)
    // 301-350 - ульевые названия звезд (Fraction 6)
    // 351-400 - древние названия звезд (Fraction 7)
    // первоначально звезде и звездной системе присваивается случайный префикс, затем минус, затем XXYY - которые берутся из oldX и oldY звездной системы 
    // пример "HIP-0749" при oldX=7 и oldY=49. по необходимости в литерал добавить первый ноль
    // названия планет по порядку от звезды (без буквы a - она для звезды), например - HIP-0749 b, HIP-0749 c и тд
    // название лун соответственно HIP-0749 b 1, HIP-0749 b 2
    // безпрефиксные имена будут присваиваться отдельно, по мере написания фракций. реализация этого отложена на потом
    // 401-800 - латинские названия планет/лун (Fraction 1)
    // 801-1200 - ацтекские названия планет/лун (Fraction 2)
    // 1201-1600 - скандинавские названия планет/лун (Fraction 3)
    // 1601-2000 - греческие названия планет/лун (Fraction 4)
    // 2001-2400 - машинные названия планет/лун (Fraction 5)
    // 2401-2800 - ульевые названия планет/лун (Fraction 6)
    // 2801-3200 - древние названия планет/лун (Fraction 7)
    //  — корабли
    //  — оборудование
    //  — товары
    //  — имена персон
    //  — фамилии персон
    //  — интерфейс
    //  — всякое разное
    private const int StarRangeStart = 0;
    private const int StarRangeEnd = 49;

    private static readonly List<LocalizationChunk> _chunks = new();
    private static readonly Dictionary<int, string> _dynamicEntries = new();
    private static int _nextDynamicId = -1;
    private static string[] _starNames = Array.Empty<string>();
    private static bool _isInitialized;
    private static bool _starNamesPrepared;
    public static IReadOnlyList<string> StarNames => _starNames;

    public static IReadOnlyList<LocalizationChunk> Chunks => _chunks;
    public static bool IsInitialized => _isInitialized;

    public static void Initialize(string directoryPath)
    {
        var reader = new LocalizationReader();

        _chunks.Clear();
        ResetDynamicValuesCore();

        foreach (var chunk in reader.ReadEnglishChunks(directoryPath))
        {
            _chunks.Add(chunk);
        }

        _chunks.Sort((a, b) => a.StartId.CompareTo(b.StartId));
        _isInitialized = true;
        _starNamesPrepared = false;
    }

    public static bool TryGet(int id, out string value)
    {
        if (id < 0)
        {
            if (_dynamicEntries.TryGetValue(id, out value))
                return true;

            value = string.Empty;
            return false;
        }

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

    public static string GetStarName(int index)
    {
        return GetStarName(index, float.NaN, float.NaN);
    }

    public static string GetStarName(int index, float oldX, float oldY)
    {
        EnsureInitialized();

        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (!_starNamesPrepared || index >= _starNames.Length)
            PrepareStarNames(index + 1);

        var root = _starNames[index];
        return ComposeStarName(root, oldX, oldY);
    }

    public static bool TryGetStarName(int index, float oldX, float oldY, out string value)
    {
        if (!IsInitialized || index < 0)
        {
            value = string.Empty;
            return false;
        }

        if (!_starNamesPrepared || index >= _starNames.Length)
            PrepareStarNames(index + 1);

        value = ComposeStarName(_starNames[index], oldX, oldY);
        return true;
    }

    public static void PrepareStarNames(int requiredCount)
    {
        EnsureInitialized();

        if (requiredCount <= 0)
        {
            _starNames = Array.Empty<string>();
            _starNamesPrepared = true;
            return;
        }

        var baseNames = new List<string>();
        foreach (var chunk in _chunks)
        {
            if (chunk.EndId < StarRangeStart || chunk.StartId > StarRangeEnd)
                continue;

            var values = chunk.Values;
            for (int i = 0; i < values.Count; i++)
            {
                var value = values[i]?.Trim();
                if (!string.IsNullOrWhiteSpace(value))
                    baseNames.Add(value);
            }
        }

        if (baseNames.Count == 0)
            throw new InvalidOperationException("No star names found in localization data.");

        _starNames = new string[requiredCount];
        int baseCount = baseNames.Count;
        for (int i = 0; i < requiredCount; i++)
            _starNames[i] = baseNames[i % baseCount];

        _starNamesPrepared = true;
    }

    private static string ComposeStarName(string root, float oldX, float oldY)
    {
        root = string.IsNullOrWhiteSpace(root) ? "STAR" : root.Trim();

        if (!IsUsableCoordinate(oldX) || !IsUsableCoordinate(oldY))
            return root;

        var x = FormatCoordinate(oldX);
        var y = FormatCoordinate(oldY);

        if (x == null || y == null)
            return root;

        return $"{root}-{x}{y}";
    }

    private static string FormatCoordinate(float coord)
    {
        if (!IsUsableCoordinate(coord))
            return null;

        var rounded = (int)Math.Round(coord, MidpointRounding.AwayFromZero);
        rounded = Math.Abs(rounded) % 100;

        return rounded.ToString("00", CultureInfo.InvariantCulture);
    }

    private static bool IsUsableCoordinate(float coord) => !float.IsNaN(coord) && !float.IsInfinity(coord);

    public static string ComposePlanetName(string starName, int planetIndex)
    {
        if (string.IsNullOrWhiteSpace(starName) || planetIndex < 0)
            return string.Empty;

        var suffix = BuildPlanetSuffix(planetIndex);
        return $"{starName} {suffix}";
    }

    public static string ComposeMoonName(string starName, int planetIndex, int moonIndex)
    {
        if (string.IsNullOrWhiteSpace(starName) || planetIndex < 0 || moonIndex < 0)
            return string.Empty;

        var suffix = BuildPlanetSuffix(planetIndex);
        var moonNumber = moonIndex + 1;
        return $"{starName} {suffix} {moonNumber}";
    }

    public static int RegisterDynamicValue(string value)
    {
        EnsureInitialized();

        if (string.IsNullOrWhiteSpace(value))
            return int.MinValue;

        var id = _nextDynamicId--;
        _dynamicEntries[id] = value.Trim();
        return id;
    }

    public static void ResetDynamicValues()
    {
        EnsureInitialized();
        ResetDynamicValuesCore();
    }

    private static void ResetDynamicValuesCore()
    {
        _dynamicEntries.Clear();
        _nextDynamicId = -1;
    }

    private static string BuildPlanetSuffix(int planetIndex)
    {
        // Astronomical convention: star = "a", planets start from "b".
        var number = planetIndex + 2; // planetIndex 0 -> column 2 -> 'b'
        Span<char> buffer = stackalloc char[8];
        int pos = buffer.Length;

        while (number > 0)
        {
            number--;
            buffer[--pos] = (char)('a' + (number % 26));
            number /= 26;
        }

        return new string(buffer[pos..]);
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
