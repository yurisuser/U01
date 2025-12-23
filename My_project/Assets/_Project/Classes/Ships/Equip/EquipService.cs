using System;
using _Project.DataAccess; // для каталогов
using _Project.Items; // для ItemType

namespace _Project.Scripts.Ships
{
    public static class EquipService
    {
        /// <summary>Собирает комплект оборудования по каталогу корабля.</summary>
        public static InstalledEquip BuildEquip(in CatalogShip shipCatalog, int quality)
        {
            return GetFullEquip(in shipCatalog); // тестовая сборка
        }

        /// <summary>Оркестратор: собирает полный комплект из тестового оборудования.</summary>
        public static InstalledEquip GetFullEquip(in CatalogShip shipCatalog)
        {
            var equip = new InstalledEquip(); // новый набор оборудования
            equip.Init(shipCatalog.WeaponSlots); // число слотов берём из каталога
            var weaponSlot = GetWeaponSlot(); // тестовое оружие
            for (int i = 0; i < equip.WeaponSlotsCount; i++)
                equip.SetWeaponSlot(i, in weaponSlot);

            equip.Engine = GetEngineSlot();
            equip.Shield = GetShieldSlot();
            equip.Scanner = GetScannerSlot();

            return equip;
        }

        private static EquipSlot GetWeaponSlot()
        {
            var list = WeaponCatalogReader.GetAll();
            if (list == null || list.Count == 0)
                return default;

            var catalog = list[0];
            return new EquipSlot
            {
                Type = ItemType.Weapon,
                Id = catalog.Id,
                Weapon = new WeaponEntity
                {
                    Damage = catalog.Damage,
                    Range = catalog.Range,
                    Rate = (int)Math.Round(catalog.RatePerSecond)
                }
            };
        }

        private static EquipSlot GetEngineSlot()
        {
            var list = EngineCatalogReader.GetAll();
            if (list == null || list.Count == 0)
                return default;

            var catalog = list[0];
            return new EquipSlot
            {
                Type = ItemType.Engine,
                Id = catalog.Id,
                Engine = new EngineEntity
                {
                    MaxSpeed = catalog.Speed,
                    Acceleration = 0f,
                    Agility = 0f
                }
            };
        }

        private static EquipSlot GetShieldSlot()
        {
            var list = ShieldCatalogReader.GetAll();
            if (list == null || list.Count == 0)
                return default;

            var catalog = list[0];
            return new EquipSlot
            {
                Type = ItemType.Shield,
                Id = catalog.Id,
                Shield = new ShieldEntity
                {
                    Radius = catalog.Radius,
                    Volume = catalog.Volume,
                    Regen = catalog.Regen
                }
            };
        }

        private static EquipSlot GetScannerSlot()
        {
            var list = ScannerCatalogReader.GetAll();
            if (list == null || list.Count == 0)
                return default;

            var catalog = list[0];
            return new EquipSlot
            {
                Type = ItemType.Scanner,
                Id = catalog.Id,
                Scanner = new ScannerEntity
                {
                    Radius = catalog.Radius
                }
            };
        }
    }
}
