using System;
using System.Collections.Generic;
using System.Globalization;

public static class LocalizationDatabase
{
    // Префиксы имён звёзд читаются из names.<lang>.star_prefix.json и размножаются под нужное количество систем.
    // Планетарные/лунные имена строятся динамически от имени звезды и регистрируются как динамические значения.
    private static readonly Dictionary<int, string> _dynamicEntries = new();
    private static int _nextDynamicId = -1;
    private static string[] _starNames = Array.Empty<string>();
    private static IReadOnlyList<string> _starPrefixes = Array.Empty<string>();
    private static bool _isInitialized;
    private static bool _starNamesPrepared;
    public static IReadOnlyList<string> StarNames => _starNames;

    public static bool IsInitialized => _isInitialized;

    public static void Initialize(string directoryPath)
    {
        var reader = new LocalizationReader();

        ResetDynamicValuesCore();
        _starPrefixes = reader.ReadStarPrefixes(directoryPath);

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

        if (!_starNamesPrepared || id >= _starNames.Length)
            PrepareStarNames(id + 1);

        if (id >= 0 && id < _starNames.Length)
        {
            value = _starNames[id];
            return true;
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

    public static int RegisterDynamicValue(string value)
    {
        EnsureInitialized();

        if (string.IsNullOrWhiteSpace(value))
            return int.MinValue;

        var id = _nextDynamicId--;
        _dynamicEntries[id] = value;
        return id;
    }

    public static void ResetDynamicValues()
    {
        ResetDynamicValuesCore();
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

        if (_starPrefixes == null || _starPrefixes.Count == 0)
            throw new InvalidOperationException("Не найдены префиксы имён звёзд в локализации.");

        _starNames = new string[requiredCount];
        int baseCount = _starPrefixes.Count;
        for (int i = 0; i < requiredCount; i++)
        {
            var value = _starPrefixes[i % baseCount];
            _starNames[i] = string.IsNullOrWhiteSpace(value) ? "STAR" : value.Trim();
        }

        _starNamesPrepared = true;
    }

    public static string ComposePlanetName(string starName, int planetIndex)
    {
        if (string.IsNullOrWhiteSpace(starName))
            return string.Empty;

        var suffix = BuildPlanetSuffix(planetIndex);
        return $"{starName} {suffix}";
    }

    public static string ComposeMoonName(string planetName, int planetIndex, int moonIndex)
    {
        if (string.IsNullOrWhiteSpace(planetName))
            return string.Empty;

        var suffix = BuildPlanetSuffix(planetIndex);
        return $"{planetName} {suffix} {moonIndex + 1}";
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
}
