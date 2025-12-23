namespace _Project.Scripts.Ships
{
    public static class EquipService
    {
        /// <summary>Собирает комплект оборудования без изменения корабля.</summary>
        public static InstalledEquip BuildEquip(in Ship ship, int quality)
        {
            var equip = new InstalledEquip(); // новый набор оборудования
            equip.Init(ship.Equipment.WeaponSlotsCount); // берём число слотов из корабля
            return equip; // подбор модулей добавим позже
        }

        private static InstalledEquip GetFullEquip()
        {
            return new InstalledEquip();
        }

        private static WeaponEntity GetWeapon()
        {
            return new WeaponEntity();
        }

        private static EngineEntity GetEngine()
        {
            return new EngineEntity();
        }

        private static ShieldEntity GetShield()
        {
            return new ShieldEntity();
        }

        private static ScannerEntity GetScanner()
        {
            return new ScannerEntity();
        }
        //делай для каждого типа оборуд свой гет
    }
}
