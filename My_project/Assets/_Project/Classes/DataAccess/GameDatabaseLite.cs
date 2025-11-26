using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Mono.Data.Sqlite;
using UnityEngine;

namespace _Project.DataAccess
{
    /// <summary>Обёртка над SQLite для чтения каталогов оружия и кораблей.</summary>
    public static class GameDatabaseLite
    {
        private const string RelativePath = "Data/game.db";
        private const string SqliteHeader = "SQLite format 3\0";

        private static string _fullPath;
        private static IReadOnlyList<CatalogWeapon> _weapons;
        private static IReadOnlyList<CatalogShip> _ships;

        /// <summary>Возвращает список оружия из базы (с кешированием).</summary>
        public static IReadOnlyList<CatalogWeapon> GetWeapons(bool forceReload = false)
        {
            if (!forceReload && _weapons != null) return _weapons;
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, key, display_name, description, damage, rate_per_second, range FROM weapons ORDER BY id";

            var list = new List<CatalogWeapon>();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var key = reader.GetString(1);
                    var displayName = reader.GetString(2);
                    var description = reader.GetString(3);
                    var damage = (float)reader.GetDouble(4);
                    var ratePerSecond = (float)reader.GetDouble(5);
                    var range = (float)reader.GetDouble(6);
                    list.Add(new CatalogWeapon(id, key, displayName, description, damage, ratePerSecond, range));
                }
            }

            _weapons = list;
            return list;
        }

        /// <summary>Возвращает список кораблей из базы (с кешированием).</summary>
        public static IReadOnlyList<CatalogShip> GetShips(bool forceReload = false)
        {
            if (!forceReload && _ships != null) return _ships;
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, key, display_name, description, hp, max_speed, agility, weapon_slots FROM ships ORDER BY id";

            var list = new List<CatalogShip>();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var key = reader.GetString(1);
                    var displayName = reader.GetString(2);
                    var description = reader.GetString(3);
                    var hp = reader.GetInt32(4);
                    var maxSpeed = (float)reader.GetDouble(5);
                    var agility = (float)reader.GetDouble(6);
                    var weaponSlots = Convert.ToByte(reader.GetInt32(7));
                    list.Add(new CatalogShip(id, key, displayName, description, hp, maxSpeed, agility, weaponSlots));
                }
            }

            _ships = list;
            return list;
        }

        private static IDbConnection OpenConnection()
        {
            var path = ResolvePath();
            EnsureDatabase(path);
            var conn = new SqliteConnection($"URI=file:{path}");
            conn.Open();
            return conn;
        }

        private static string ResolvePath()
        {
            if (!string.IsNullOrEmpty(_fullPath)) return _fullPath;
            var p = Path.Combine(Application.dataPath, RelativePath);
            var dir = Path.GetDirectoryName(p);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            _fullPath = p;
            return p;
        }

        private static void EnsureDatabase(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var isValidSqlite = File.Exists(path) && IsSqliteFile(path);
            if (!isValidSqlite)
            {
                if (File.Exists(path)) File.Delete(path);
                using (var conn = new SqliteConnection($"URI=file:{path}"))
                {
                    conn.Open(); // creates file
                }
            }

            using var connection = new SqliteConnection($"URI=file:{path}");
            connection.Open();
            CreateSchema(connection);
            SeedDefaults(connection);
        }

        private static bool IsSqliteFile(string path)
        {
            try
            {
                using var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                if (fs.Length < SqliteHeader.Length) return false;
                Span<byte> buffer = stackalloc byte[SqliteHeader.Length];
                var read = fs.Read(buffer);
                if (read < SqliteHeader.Length) return false;
                var header = Encoding.ASCII.GetString(buffer);
                return header == SqliteHeader;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        private static void CreateSchema(IDbConnection connection)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS weapons (
    id INTEGER PRIMARY KEY,
    key TEXT NOT NULL UNIQUE,
    display_name TEXT NOT NULL,
    description TEXT NOT NULL,
    damage REAL NOT NULL,
    rate_per_second REAL NOT NULL,
    range REAL NOT NULL
);
CREATE TABLE IF NOT EXISTS ships (
    id INTEGER PRIMARY KEY,
    key TEXT NOT NULL UNIQUE,
    display_name TEXT NOT NULL,
    description TEXT NOT NULL,
    hp INTEGER NOT NULL,
    max_speed REAL NOT NULL,
    agility REAL NOT NULL,
    weapon_slots INTEGER NOT NULL
);
";
            cmd.ExecuteNonQuery();
        }

        private static void SeedDefaults(IDbConnection connection)
        {
            using var tx = connection.BeginTransaction();
            using var cmd = connection.CreateCommand();
            cmd.Transaction = tx;

            cmd.CommandText = @"
INSERT OR IGNORE INTO weapons (id, key, display_name, description, damage, rate_per_second, range) VALUES
    (1, 'laser_basic', 'Базовый лазер', 'Старый образец корабельного лазера.', 12.0, 1.5, 50.0),
    (2, 'railgun_mk1', 'Рельсотрон MK1', 'Пробивает броню, но стреляет медленно.', 35.0, 0.5, 120.0);

INSERT OR IGNORE INTO ships (id, key, display_name, description, hp, max_speed, agility, weapon_slots) VALUES
    (1, 'scout', 'Разведчик', 'Лёгкий корабль для быстрых рейдов.', 150, 28.0, 0.8, 2),
    (2, 'frigate', 'Фрегат', 'Универсальный боевой корабль.', 420, 18.0, 0.5, 4);
";
            cmd.ExecuteNonQuery();
            tx.Commit();
        }
    }
}
