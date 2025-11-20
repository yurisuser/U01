using System.Collections.Generic;
using System.IO;
using LiteDB;
using UnityEngine;

namespace _Project.DataAccess
{
    /// <summary>Обёртка над LiteDB для чтения каталогов оружия и кораблей.</summary>
    public static class GameDatabaseLite
    {
        private const string RelativePath = "Data/game.db";

        private static string _fullPath;
        private static IReadOnlyList<CatalogWeapon> _weapons;
        private static IReadOnlyList<CatalogShip> _ships;

        /// <summary>Возвращает список оружия из базы (с кешированием).</summary>
        public static IReadOnlyList<CatalogWeapon> GetWeapons(bool forceReload = false)
        {
            if (!forceReload && _weapons != null) return _weapons;
            using var db = OpenReadOnly();
            var col = db.GetCollection<WeaponDoc>("weapons");
            var list = new List<CatalogWeapon>();
            foreach (var d in col.Find(Query.All("_id", Query.Ascending)))
                list.Add(new CatalogWeapon(d.Id, d.Key, d.DisplayName, d.Description, d.Damage, d.RatePerSecond, d.Range));
            _weapons = list;
            return list;
        }

        /// <summary>Возвращает список кораблей из базы (с кешированием).</summary>
        public static IReadOnlyList<CatalogShip> GetShips(bool forceReload = false)
        {
            if (!forceReload && _ships != null) return _ships;
            using var db = OpenReadOnly();
            var col = db.GetCollection<ShipDoc>("ships");
            var list = new List<CatalogShip>();
            foreach (var d in col.Find(Query.All("_id", Query.Ascending)))
                list.Add(new CatalogShip(d.Id, d.Key, d.DisplayName, d.Description, d.Hp, d.MaxSpeed, d.Agility, d.WeaponSlots));
            _ships = list;
            return list;
        }

        private static LiteDatabase OpenReadOnly()
        {
            var path = ResolvePath();
            if (!File.Exists(path)) SeedDatabase(path);
            return new LiteDatabase(new ConnectionString { Filename = path, ReadOnly = true });
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

        private static void SeedDatabase(string path)
        {
            using var db = new LiteDatabase(path);
            var weapons = db.GetCollection<WeaponDoc>("weapons");
            weapons.EnsureIndex(x => x.Id, true);
            weapons.InsertBulk(new[]
            {
                new WeaponDoc{ Id=1, Key="laser_basic", DisplayName="Базовый лазер", Description="Старый образец корабельного лазера.", Damage=12f, RatePerSecond=1.5f, Range=50f },
                new WeaponDoc{ Id=2, Key="railgun_mk1", DisplayName="Рельсотрон MK1", Description="Пробивает броню, но стреляет медленно.", Damage=35f, RatePerSecond=0.5f, Range=120f }
            });

            var ships = db.GetCollection<ShipDoc>("ships");
            ships.EnsureIndex(x => x.Id, true);
            ships.InsertBulk(new[]
            {
                new ShipDoc{ Id=1, Key="scout", DisplayName="Разведчик", Description="Лёгкий корабль для быстрых рейдов.", Hp=150, MaxSpeed=28f, Agility=0.8f, WeaponSlots=2 },
                new ShipDoc{ Id=2, Key="frigate", DisplayName="Фрегат", Description="Универсальный боевой корабль.", Hp=420, MaxSpeed=18f, Agility=0.5f, WeaponSlots=4 }
            });
        }

        private sealed class WeaponDoc
        {
            public int Id { get; set; }
            public string Key { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public float Damage { get; set; }
            public float RatePerSecond { get; set; }
            public float Range { get; set; }
        }

        private sealed class ShipDoc
        {
            public int Id { get; set; }
            public string Key { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public int Hp { get; set; }
            public float MaxSpeed { get; set; }
            public float Agility { get; set; }
            public byte WeaponSlots { get; set; }
        }
    }
}
