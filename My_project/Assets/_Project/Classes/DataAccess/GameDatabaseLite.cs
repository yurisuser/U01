using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Mono.Data.Sqlite;
using UnityEngine;

namespace _Project.DataAccess
{
    /// <summary>Обёртка над SQLite для чтения каталогов предметов и кораблей.</summary>
    public static class GameDatabaseLite
    {
        private const string RelativePath = "Data/game.db";
        private const string SqliteHeader = "SQLite format 3\0";
        private static string _fullPath;
        private static IReadOnlyList<CatalogWeapon> _weapons;
        private static IReadOnlyList<CatalogGoods> _goods;
        private static IReadOnlyList<CatalogAmmo> _ammo;
        private static IReadOnlyList<CatalogQuest> _quest;
        private static IReadOnlyList<CatalogEngine> _engines;
        private static IReadOnlyList<CatalogScanner> _scanners;
        private static IReadOnlyList<CatalogShield> _shields;
        private static IReadOnlyList<CatalogShip> _ships;

        /// <summary>Возвращает список оружия из базы (с кешированием).</summary>
        public static IReadOnlyList<CatalogWeapon> GetWeapons(bool forceReload = false)
        {
            if (!forceReload && _weapons != null) return _weapons;
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, key, display_name, description, price, weight, stackable, max_stack, tech_level, power_use, cpu_use, damage, rate_per_second, range FROM weapons ORDER BY id";

            var list = new List<CatalogWeapon>();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var key = reader.GetString(1);
                    var displayName = reader.GetString(2);
                    var description = reader.GetString(3);
                    var price = reader.GetInt32(4);
                    var weight = (float)reader.GetDouble(5);
                    var stackable = reader.GetInt32(6) != 0;
                    var maxStack = reader.GetInt32(7);
                    var techLevel = reader.GetInt32(8);
                    var powerUse = (float)reader.GetDouble(9);
                    var cpuUse = (float)reader.GetDouble(10);
                    var damage = (float)reader.GetDouble(11);
                    var ratePerSecond = (float)reader.GetDouble(12);
                    var range = (float)reader.GetDouble(13);
                    list.Add(new CatalogWeapon(id, key, displayName, description, price, weight, stackable, maxStack, techLevel, powerUse, cpuUse, damage, ratePerSecond, range));
                }
            }

            _weapons = list;
            return list;
        }

        /// <summary>Возвращает список товаров из базы (с кешированием).</summary>
        public static IReadOnlyList<CatalogGoods> GetGoods(bool forceReload = false)
        {
            if (!forceReload && _goods != null) return _goods;
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, key, display_name, description, price, weight, stackable, max_stack FROM goods ORDER BY id";

            var list = new List<CatalogGoods>();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var key = reader.GetString(1);
                    var displayName = reader.GetString(2);
                    var description = reader.GetString(3);
                    var price = reader.GetInt32(4);
                    var weight = (float)reader.GetDouble(5);
                    var stackable = reader.GetInt32(6) != 0;
                    var maxStack = reader.GetInt32(7);
                    list.Add(new CatalogGoods(id, key, displayName, description, price, weight, stackable, maxStack));
                }
            }

            _goods = list;
            return list;
        }

        /// <summary>Возвращает список боеприпасов из базы (с кешированием).</summary>
        public static IReadOnlyList<CatalogAmmo> GetAmmo(bool forceReload = false)
        {
            if (!forceReload && _ammo != null) return _ammo;
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, key, display_name, description, price, weight, stackable, max_stack FROM ammo ORDER BY id";

            var list = new List<CatalogAmmo>();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var key = reader.GetString(1);
                    var displayName = reader.GetString(2);
                    var description = reader.GetString(3);
                    var price = reader.GetInt32(4);
                    var weight = (float)reader.GetDouble(5);
                    var stackable = reader.GetInt32(6) != 0;
                    var maxStack = reader.GetInt32(7);
                    list.Add(new CatalogAmmo(id, key, displayName, description, price, weight, stackable, maxStack));
                }
            }

            _ammo = list;
            return list;
        }

        /// <summary>Возвращает список квестовых предметов из базы (с кешированием).</summary>
        public static IReadOnlyList<CatalogQuest> GetQuest(bool forceReload = false)
        {
            if (!forceReload && _quest != null) return _quest;
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, key, display_name, description, price, weight, stackable, max_stack FROM quest ORDER BY id";

            var list = new List<CatalogQuest>();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var key = reader.GetString(1);
                    var displayName = reader.GetString(2);
                    var description = reader.GetString(3);
                    var price = reader.GetInt32(4);
                    var weight = (float)reader.GetDouble(5);
                    var stackable = reader.GetInt32(6) != 0;
                    var maxStack = reader.GetInt32(7);
                    list.Add(new CatalogQuest(id, key, displayName, description, price, weight, stackable, maxStack));
                }
            }

            _quest = list;
            return list;
        }

        /// <summary>Возвращает список двигателей из базы (с кешированием).</summary>
        public static IReadOnlyList<CatalogEngine> GetEngines(bool forceReload = false)
        {
            if (!forceReload && _engines != null) return _engines;
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, key, display_name, description, price, weight, stackable, max_stack, tech_level, power_use, cpu_use, speed FROM engines ORDER BY id";

            var list = new List<CatalogEngine>();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var key = reader.GetString(1);
                    var displayName = reader.GetString(2);
                    var description = reader.GetString(3);
                    var price = reader.GetInt32(4);
                    var weight = (float)reader.GetDouble(5);
                    var stackable = reader.GetInt32(6) != 0;
                    var maxStack = reader.GetInt32(7);
                    var techLevel = reader.GetInt32(8);
                    var powerUse = (float)reader.GetDouble(9);
                    var cpuUse = (float)reader.GetDouble(10);
                    var speed = (float)reader.GetDouble(11);
                    list.Add(new CatalogEngine(id, key, displayName, description, price, weight, stackable, maxStack, techLevel, powerUse, cpuUse, speed));
                }
            }

            _engines = list;
            return list;
        }

        /// <summary>Возвращает список сканеров из базы (с кешированием).</summary>
        public static IReadOnlyList<CatalogScanner> GetScanners(bool forceReload = false)
        {
            if (!forceReload && _scanners != null) return _scanners;
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, key, display_name, description, price, weight, stackable, max_stack, tech_level, power_use, cpu_use, radius FROM scanners ORDER BY id";

            var list = new List<CatalogScanner>();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var key = reader.GetString(1);
                    var displayName = reader.GetString(2);
                    var description = reader.GetString(3);
                    var price = reader.GetInt32(4);
                    var weight = (float)reader.GetDouble(5);
                    var stackable = reader.GetInt32(6) != 0;
                    var maxStack = reader.GetInt32(7);
                    var techLevel = reader.GetInt32(8);
                    var powerUse = (float)reader.GetDouble(9);
                    var cpuUse = (float)reader.GetDouble(10);
                    var radius = (float)reader.GetDouble(11);
                    list.Add(new CatalogScanner(id, key, displayName, description, price, weight, stackable, maxStack, techLevel, powerUse, cpuUse, radius));
                }
            }

            _scanners = list;
            return list;
        }

        /// <summary>Возвращает список щитов из базы (с кешированием).</summary>
        public static IReadOnlyList<CatalogShield> GetShields(bool forceReload = false)
        {
            if (!forceReload && _shields != null) return _shields;
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, key, display_name, description, price, weight, stackable, max_stack, tech_level, power_use, cpu_use, radius, volume, regen FROM shields ORDER BY id";

            var list = new List<CatalogShield>();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var key = reader.GetString(1);
                    var displayName = reader.GetString(2);
                    var description = reader.GetString(3);
                    var price = reader.GetInt32(4);
                    var weight = (float)reader.GetDouble(5);
                    var stackable = reader.GetInt32(6) != 0;
                    var maxStack = reader.GetInt32(7);
                    var techLevel = reader.GetInt32(8);
                    var powerUse = (float)reader.GetDouble(9);
                    var cpuUse = (float)reader.GetDouble(10);
                    var radius = (float)reader.GetDouble(11);
                    var volume = (float)reader.GetDouble(12);
                    var regen = (float)reader.GetDouble(13);
                    list.Add(new CatalogShield(id, key, displayName, description, price, weight, stackable, maxStack, techLevel, powerUse, cpuUse, radius, volume, regen));
                }
            }

            _shields = list;
            return list;
        }

        /// <summary>Возвращает список кораблей из базы (с кешированием).</summary>
        public static IReadOnlyList<CatalogShip> GetShips(bool forceReload = false)
        {
            if (!forceReload && _ships != null) return _ships;
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, key, display_name, description, hp, max_speed, agility, acceleration, prefab_size, prefab_name, weapon_slots FROM ships ORDER BY id";

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
                    var acceleration = (float)reader.GetDouble(7);
                    var prefabSize = (float)reader.GetDouble(8);
                    var prefabName = reader.GetString(9);
                    var weaponSlots = Convert.ToByte(reader.GetInt32(10));
                    list.Add(new CatalogShip(id, key, displayName, description, hp, maxSpeed, agility, acceleration, prefabSize, prefabName, weaponSlots));
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
            EnsureShipColumns(connection);
            EnsureWeaponsSchema(connection);
            EnsureLegacyItemsRemoval(connection);
            EnsureEquipmentColumns(connection);
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
    price INTEGER NOT NULL DEFAULT 0,
    weight REAL NOT NULL DEFAULT 1,
    stackable BOOLEAN NOT NULL DEFAULT 0 CHECK (stackable IN (0,1)),
    max_stack INTEGER NOT NULL DEFAULT 1,
    tech_level INTEGER NOT NULL DEFAULT 1,
    power_use REAL NOT NULL DEFAULT 0,
    cpu_use REAL NOT NULL DEFAULT 0,
    damage REAL NOT NULL,
    rate_per_second REAL NOT NULL,
    range REAL NOT NULL
);
CREATE TABLE IF NOT EXISTS goods (
    id INTEGER PRIMARY KEY,
    key TEXT NOT NULL UNIQUE,
    display_name TEXT NOT NULL,
    description TEXT NOT NULL,
    price INTEGER NOT NULL DEFAULT 0,
    weight REAL NOT NULL DEFAULT 1,
    stackable BOOLEAN NOT NULL DEFAULT 1 CHECK (stackable IN (0,1)),
    max_stack INTEGER NOT NULL DEFAULT 1
);
CREATE TABLE IF NOT EXISTS ammo (
    id INTEGER PRIMARY KEY,
    key TEXT NOT NULL UNIQUE,
    display_name TEXT NOT NULL,
    description TEXT NOT NULL,
    price INTEGER NOT NULL DEFAULT 0,
    weight REAL NOT NULL DEFAULT 1,
    stackable BOOLEAN NOT NULL DEFAULT 1 CHECK (stackable IN (0,1)),
    max_stack INTEGER NOT NULL DEFAULT 1
);
CREATE TABLE IF NOT EXISTS quest (
    id INTEGER PRIMARY KEY,
    key TEXT NOT NULL UNIQUE,
    display_name TEXT NOT NULL,
    description TEXT NOT NULL,
    price INTEGER NOT NULL DEFAULT 0,
    weight REAL NOT NULL DEFAULT 1,
    stackable BOOLEAN NOT NULL DEFAULT 1 CHECK (stackable IN (0,1)),
    max_stack INTEGER NOT NULL DEFAULT 1
);
CREATE TABLE IF NOT EXISTS engines (
    id INTEGER PRIMARY KEY,
    key TEXT NOT NULL UNIQUE,
    display_name TEXT NOT NULL,
    description TEXT NOT NULL,
    price INTEGER NOT NULL DEFAULT 0,
    weight REAL NOT NULL DEFAULT 1,
    stackable BOOLEAN NOT NULL DEFAULT 0 CHECK (stackable IN (0,1)),
    max_stack INTEGER NOT NULL DEFAULT 1,
    tech_level INTEGER NOT NULL DEFAULT 1,
    power_use REAL NOT NULL DEFAULT 0,
    cpu_use REAL NOT NULL DEFAULT 0,
    speed REAL NOT NULL DEFAULT 0
);
CREATE TABLE IF NOT EXISTS scanners (
    id INTEGER PRIMARY KEY,
    key TEXT NOT NULL UNIQUE,
    display_name TEXT NOT NULL,
    description TEXT NOT NULL,
    price INTEGER NOT NULL DEFAULT 0,
    weight REAL NOT NULL DEFAULT 1,
    stackable BOOLEAN NOT NULL DEFAULT 0 CHECK (stackable IN (0,1)),
    max_stack INTEGER NOT NULL DEFAULT 1,
    tech_level INTEGER NOT NULL DEFAULT 1,
    power_use REAL NOT NULL DEFAULT 0,
    cpu_use REAL NOT NULL DEFAULT 0,
    radius REAL NOT NULL DEFAULT 0
);
CREATE TABLE IF NOT EXISTS shields (
    id INTEGER PRIMARY KEY,
    key TEXT NOT NULL UNIQUE,
    display_name TEXT NOT NULL,
    description TEXT NOT NULL,
    price INTEGER NOT NULL DEFAULT 0,
    weight REAL NOT NULL DEFAULT 1,
    stackable BOOLEAN NOT NULL DEFAULT 0 CHECK (stackable IN (0,1)),
    max_stack INTEGER NOT NULL DEFAULT 1,
    tech_level INTEGER NOT NULL DEFAULT 1,
    power_use REAL NOT NULL DEFAULT 0,
    cpu_use REAL NOT NULL DEFAULT 0,
    radius REAL NOT NULL DEFAULT 0,
    volume REAL NOT NULL DEFAULT 0,
    regen REAL NOT NULL DEFAULT 0
);
CREATE TABLE IF NOT EXISTS ships (
    id INTEGER PRIMARY KEY,
    key TEXT NOT NULL UNIQUE,
    display_name TEXT NOT NULL,
    description TEXT NOT NULL,
    hp INTEGER NOT NULL,
    max_speed REAL NOT NULL,
    agility REAL NOT NULL,
    power REAL NOT NULL DEFAULT 0,
    cpu REAL NOT NULL DEFAULT 0,
    acceleration REAL NOT NULL DEFAULT 0,
    prefab_size REAL NOT NULL DEFAULT 1,
    prefab_name TEXT NOT NULL DEFAULT '',
    weapon_slots INTEGER NOT NULL,
    shield_slots INTEGER NOT NULL DEFAULT 0,
    engine_slots INTEGER NOT NULL DEFAULT 0
);
";
            cmd.ExecuteNonQuery();
        }

        private static void EnsureShipColumns(IDbConnection connection)
        {
            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA table_info(ships)";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    existing.Add(reader.GetString(1));
            }

            using var alter = connection.CreateCommand();
            if (!existing.Contains("acceleration"))
            {
                alter.CommandText = "ALTER TABLE ships ADD COLUMN acceleration REAL NOT NULL DEFAULT 0";
                alter.ExecuteNonQuery();
            }

            if (!existing.Contains("power"))
            {
                alter.CommandText = "ALTER TABLE ships ADD COLUMN power REAL NOT NULL DEFAULT 0";
                alter.ExecuteNonQuery();
            }

            if (!existing.Contains("cpu"))
            {
                alter.CommandText = "ALTER TABLE ships ADD COLUMN cpu REAL NOT NULL DEFAULT 0";
                alter.ExecuteNonQuery();
            }

            if (!existing.Contains("prefab_size"))
            {
                alter.CommandText = "ALTER TABLE ships ADD COLUMN prefab_size REAL NOT NULL DEFAULT 1";
                alter.ExecuteNonQuery();
            }

            if (!existing.Contains("prefab_name"))
            {
                alter.CommandText = "ALTER TABLE ships ADD COLUMN prefab_name TEXT NOT NULL DEFAULT ''";
                alter.ExecuteNonQuery();
            }

            if (!existing.Contains("shield_slots"))
            {
                alter.CommandText = "ALTER TABLE ships ADD COLUMN shield_slots INTEGER NOT NULL DEFAULT 0";
                alter.ExecuteNonQuery();
            }

            if (!existing.Contains("engine_slots"))
            {
                alter.CommandText = "ALTER TABLE ships ADD COLUMN engine_slots INTEGER NOT NULL DEFAULT 0";
                alter.ExecuteNonQuery();
            }
        }

        private static void EnsureLegacyItemsRemoval(IDbConnection connection)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='items'";
            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return;

            reader.Close();
            cmd.CommandText = "DROP TABLE IF EXISTS items";
            cmd.ExecuteNonQuery();
        }

        private static void EnsureWeaponsSchema(IDbConnection connection)
        {
            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var schemaCmd = connection.CreateCommand())
            {
                schemaCmd.CommandText = "PRAGMA table_info(weapons)";
                using var reader = schemaCmd.ExecuteReader();
                while (reader.Read())
                    existing.Add(reader.GetString(1));
            }

            if (existing.Count == 0)
                return;

            if (existing.Contains("item_id") || !existing.Contains("key") || !existing.Contains("display_name") || !existing.Contains("description"))
            {
                using var tx = connection.BeginTransaction();
                using var cmd = connection.CreateCommand();
                cmd.Transaction = tx;

                cmd.CommandText = @"
CREATE TABLE weapons_new (
    id INTEGER PRIMARY KEY,
    key TEXT NOT NULL UNIQUE,
    display_name TEXT NOT NULL,
    description TEXT NOT NULL,
    price INTEGER NOT NULL DEFAULT 0,
    weight REAL NOT NULL DEFAULT 1,
    stackable BOOLEAN NOT NULL DEFAULT 0 CHECK (stackable IN (0,1)),
    max_stack INTEGER NOT NULL DEFAULT 1,
    tech_level INTEGER NOT NULL DEFAULT 1,
    damage REAL NOT NULL,
    rate_per_second REAL NOT NULL,
    range REAL NOT NULL
);";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='items'";
                var hasItems = cmd.ExecuteScalar() != null;

                if (existing.Contains("item_id") && hasItems)
                {
                    cmd.CommandText = @"
INSERT INTO weapons_new (id, key, display_name, description, price, weight, stackable, max_stack, tech_level, damage, rate_per_second, range)
SELECT w.item_id, i.key, i.display_name, i.description, i.price, i.weight, i.stackable, i.max_stack, 1, w.damage, w.rate_per_second, w.range
FROM weapons w
JOIN items i ON i.id = w.item_id;";
                }
                else
                {
                    var idExpr = existing.Contains("id") ? "id" : "item_id";
                    var keyExpr = existing.Contains("key") ? "key" : "'weapon_' || " + idExpr;
                    var nameExpr = existing.Contains("display_name") ? "display_name" : "'Weapon ' || " + idExpr;
                    var descExpr = existing.Contains("description") ? "description" : "''";
                    var priceExpr = existing.Contains("price") ? "price" : "0";
                    var weightExpr = existing.Contains("weight") ? "weight" : "1";
                    var stackExpr = existing.Contains("stackable") ? "stackable" : "0";
                    var maxStackExpr = existing.Contains("max_stack") ? "max_stack" : "1";

                    cmd.CommandText = $@"
INSERT INTO weapons_new (id, key, display_name, description, price, weight, stackable, max_stack, tech_level, damage, rate_per_second, range)
SELECT {idExpr}, {keyExpr}, {nameExpr}, {descExpr}, {priceExpr}, {weightExpr}, {stackExpr}, {maxStackExpr}, 1, damage, rate_per_second, range
FROM weapons;";
                }
                cmd.ExecuteNonQuery();

                cmd.CommandText = "DROP TABLE weapons;";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "ALTER TABLE weapons_new RENAME TO weapons;";
                cmd.ExecuteNonQuery();

                tx.Commit();
                return;
            }

            using var alter = connection.CreateCommand();
            if (!existing.Contains("price"))
            {
                alter.CommandText = "ALTER TABLE weapons ADD COLUMN price INTEGER NOT NULL DEFAULT 0";
                alter.ExecuteNonQuery();
            }

            if (!existing.Contains("weight"))
            {
                alter.CommandText = "ALTER TABLE weapons ADD COLUMN weight REAL NOT NULL DEFAULT 1";
                alter.ExecuteNonQuery();
            }

            if (!existing.Contains("stackable"))
            {
                alter.CommandText = "ALTER TABLE weapons ADD COLUMN stackable BOOLEAN NOT NULL DEFAULT 0 CHECK (stackable IN (0,1))";
                alter.ExecuteNonQuery();
            }

            if (!existing.Contains("max_stack"))
            {
                alter.CommandText = "ALTER TABLE weapons ADD COLUMN max_stack INTEGER NOT NULL DEFAULT 1";
                alter.ExecuteNonQuery();
            }

            if (!existing.Contains("tech_level"))
            {
                alter.CommandText = "ALTER TABLE weapons ADD COLUMN tech_level INTEGER NOT NULL DEFAULT 1";
                alter.ExecuteNonQuery();
            }

            if (!existing.Contains("power_use"))
            {
                alter.CommandText = "ALTER TABLE weapons ADD COLUMN power_use REAL NOT NULL DEFAULT 0";
                alter.ExecuteNonQuery();
            }

            if (!existing.Contains("cpu_use"))
            {
                alter.CommandText = "ALTER TABLE weapons ADD COLUMN cpu_use REAL NOT NULL DEFAULT 0";
                alter.ExecuteNonQuery();
            }
        }

        private static void EnsureEquipmentColumns(IDbConnection connection)
        {
            EnsureColumns(connection, "engines", new[]
            {
                ("tech_level", "INTEGER NOT NULL DEFAULT 1"),
                ("power_use", "REAL NOT NULL DEFAULT 0"),
                ("cpu_use", "REAL NOT NULL DEFAULT 0"),
                ("speed", "REAL NOT NULL DEFAULT 0")
            });
            EnsureColumns(connection, "scanners", new[]
            {
                ("tech_level", "INTEGER NOT NULL DEFAULT 1"),
                ("power_use", "REAL NOT NULL DEFAULT 0"),
                ("cpu_use", "REAL NOT NULL DEFAULT 0"),
                ("radius", "REAL NOT NULL DEFAULT 0")
            });
            EnsureColumns(connection, "shields", new[]
            {
                ("tech_level", "INTEGER NOT NULL DEFAULT 1"),
                ("power_use", "REAL NOT NULL DEFAULT 0"),
                ("cpu_use", "REAL NOT NULL DEFAULT 0"),
                ("radius", "REAL NOT NULL DEFAULT 0"),
                ("volume", "REAL NOT NULL DEFAULT 0"),
                ("regen", "REAL NOT NULL DEFAULT 0")
            });
        }

        private static void EnsureColumns(IDbConnection connection, string table, (string Name, string Sql)[] columns)
        {
            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $"PRAGMA table_info({table})";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    existing.Add(reader.GetString(1));
            }

            using var alter = connection.CreateCommand();
            foreach (var col in columns)
            {
                if (existing.Contains(col.Name))
                    continue;
                alter.CommandText = $"ALTER TABLE {table} ADD COLUMN {col.Name} {col.Sql}";
                alter.ExecuteNonQuery();
            }
        }

        private static void SeedDefaults(IDbConnection connection)
        {
            using var tx = connection.BeginTransaction();
            using var cmd = connection.CreateCommand();
            cmd.Transaction = tx;

            cmd.CommandText = @"
INSERT OR IGNORE INTO weapons (id, key, display_name, description, price, weight, stackable, max_stack, tech_level, power_use, cpu_use, damage, rate_per_second, range) VALUES
    (1, 'laser_basic', 'Базовый лазер', 'Старый образец корабельного лазера.', 100, 1, 0, 1, 1, 5, 2, 12.0, 1.5, 50.0),
    (2, 'railgun_mk1', 'Рельсотрон MK1', 'Пробивает броню, но стреляет медленно.', 300, 1, 0, 1, 2, 8, 3, 35.0, 0.5, 120.0);

INSERT OR IGNORE INTO goods (id, key, display_name, description, price, weight, stackable, max_stack) VALUES
    (1, 'test_goods', 'Тестовый товар', 'Тестовый груз для проверки.', 10, 1, 1, 50);

INSERT OR IGNORE INTO ammo (id, key, display_name, description, price, weight, stackable, max_stack) VALUES
    (1, 'test_ammo', 'Тестовые боеприпасы', 'Тестовые патроны для проверки.', 5, 0.2, 1, 200);

INSERT OR IGNORE INTO quest (id, key, display_name, description, price, weight, stackable, max_stack) VALUES
    (1, 'test_quest', 'Тестовый квестовый предмет', 'Квестовый предмет для проверки.', 0, 0.5, 1, 10);

INSERT OR IGNORE INTO engines (id, key, display_name, description, price, weight, stackable, max_stack, tech_level, power_use, cpu_use, speed) VALUES
    (1, 'test_engine', 'Тестовый двигатель', 'Двигатель для проверки.', 200, 5, 0, 1, 1, 10, 4, 10.0);

INSERT OR IGNORE INTO scanners (id, key, display_name, description, price, weight, stackable, max_stack, tech_level, power_use, cpu_use, radius) VALUES
    (1, 'test_scanner', 'Тестовый сканер', 'Сканер для проверки.', 150, 2, 0, 1, 1, 3, 6, 100.0);

INSERT OR IGNORE INTO shields (id, key, display_name, description, price, weight, stackable, max_stack, tech_level, power_use, cpu_use, radius, volume, regen) VALUES
    (1, 'test_shield', 'Тестовый щит', 'Щит для проверки.', 250, 4, 0, 1, 1, 7, 5, 25.0, 300.0, 5.0);

INSERT OR IGNORE INTO ships (id, key, display_name, description, hp, max_speed, agility, power, cpu, acceleration, prefab_size, prefab_name, weapon_slots, shield_slots, engine_slots) VALUES
    (1, 'scout', 'Разведчик', 'Лёгкий корабль для быстрых рейдов.', 150, 28.0, 0.8, 50, 40, 0, 1.0, '', 2, 1, 1),
    (2, 'frigate', 'Фрегат', 'Универсальный боевой корабль.', 420, 18.0, 0.5, 120, 90, 0, 1.0, '', 4, 2, 1);
";
            cmd.ExecuteNonQuery();
            tx.Commit();
        }
    }
}
