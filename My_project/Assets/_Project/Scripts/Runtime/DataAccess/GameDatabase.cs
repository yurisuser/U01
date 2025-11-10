using System;
using System.Collections.Generic;
using System.IO;
using Mono.Data.Sqlite;
using UnityEngine;

namespace _Project.DataAccess
{
    /// <summary>
    /// Простое чтение данных из Assets/Data/game.db через Mono.Data.Sqlite.
    /// </summary>
    public static class GameDatabase
    {
        private const string DatabaseRelativePath = "Data/game.db";

        private static string? _cachedPath;
        private static IReadOnlyList<WeaponEntity>? _weaponsCache;
        private static IReadOnlyList<ShipEntity>? _shipsCache;
        private static IReadOnlyList<LocalizationEntry>? _localizationCache;

        /// <summary>
        /// Возвращает все записи из таблицы Weapons. Передай forceReload=true, если обновил файл БД.
        /// </summary>
        public static IReadOnlyList<WeaponEntity> GetWeapons(bool forceReload = false)
        {
            if (!forceReload && _weaponsCache != null)
            {
                return _weaponsCache;
            }

            var result = new List<WeaponEntity>();
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT Id, Key, DisplayName, Description, Damage, RatePerSecond, Range
                                    FROM Weapons
                                    ORDER BY Id;";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var row = new WeaponEntity(
                    id: reader.GetInt32(0),
                    key: reader.GetString(1),
                    displayName: reader.GetString(2),
                    description: reader.GetString(3),
                    damage: Convert.ToSingle(reader.GetDouble(4)),
                    ratePerSecond: Convert.ToSingle(reader.GetDouble(5)),
                    range: Convert.ToSingle(reader.GetDouble(6)));

                result.Add(row);
            }

            _weaponsCache = result;
            return result;
        }

        /// <summary>
        /// Возвращает все записи из таблицы ShipTemplates. forceReload=true перечитает файл.
        /// </summary>
        public static IReadOnlyList<ShipEntity> GetShips(bool forceReload = false)
        {
            if (!forceReload && _shipsCache != null)
            {
                return _shipsCache;
            }

            var result = new List<ShipEntity>();
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT Id, Key, DisplayName, Description, Hp, MaxSpeed, Agility, WeaponSlots
                                    FROM ShipTemplates
                                    ORDER BY Id;";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var row = new ShipEntity(
                    id: reader.GetInt32(0),
                    key: reader.GetString(1),
                    displayName: reader.GetString(2),
                    description: reader.GetString(3),
                    hp: reader.GetInt32(4),
                    maxSpeed: Convert.ToSingle(reader.GetDouble(5)),
                    agility: Convert.ToSingle(reader.GetDouble(6)),
                    weaponSlots: (byte)reader.GetInt32(7));

                result.Add(row);
            }

            _shipsCache = result;
            return result;
        }

        /// <summary>
        /// Сбрасывает кэш, чтобы перезагрузить данные после правок БД.
        /// </summary>
        public static void InvalidateCache()
        {
            _weaponsCache = null;
            _shipsCache = null;
            _localizationCache = null;
        }

        /// <summary>
        /// Возвращает все строки локализации (Id, Value) из таблицы LocalizationEntries.
        /// </summary>
        public static IReadOnlyList<LocalizationEntry> GetLocalizationEntries(bool forceReload = false)
        {
            if (!forceReload && _localizationCache != null)
            {
                return _localizationCache;
            }

            var result = new List<LocalizationEntry>();
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT Id, Value FROM LocalizationEntries ORDER BY Id;";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var entry = new LocalizationEntry(
                    reader.GetInt32(0),
                    reader.IsDBNull(1) ? string.Empty : reader.GetString(1));
                result.Add(entry);
            }

            _localizationCache = result;
            return result;
        }

        private static SqliteConnection OpenConnection()
        {
            var path = ResolveDatabasePath();
            var connection = new SqliteConnection($"URI=file:{path}");
            connection.Open();
            return connection;
        }

        private static string ResolveDatabasePath()
        {
            if (!string.IsNullOrEmpty(_cachedPath))
            {
                return _cachedPath!;
            }

            var path = Path.Combine(Application.dataPath, DatabaseRelativePath);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Не найден файл базы данных по пути {path}. Убедись, что он лежит в Assets/Data.");
            }

            _cachedPath = path;
            return path;
        }
    }
}
