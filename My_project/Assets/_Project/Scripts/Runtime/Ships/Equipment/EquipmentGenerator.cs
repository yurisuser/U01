using _Project.Scripts.NPC.Fraction; // для Fraction
using UnityEngine; // для Random.Range

namespace _Project.Scripts.Ships
{
    public static class EquipmentGenerator // минимальный генератор оборудования
    {
        public static void InitForShip(ref Ship ship, bool fillWeapons = true) // инициализация оборудования корабля
        {
            var count = GetWeaponSlotsCount(ship.Type, ship.MakerFraction); // определяем число оружейных слотов
            ship.Equipment.Init(count); // инициализируем блок оружия

            if (!fillWeapons || count == 0) return; // если наполнение не требуется — выходим

            for (int i = 0; i < count; i++) // для каждого слота
            {
                var weapon = GenerateWeapon(ship.Type, ship.MakerFraction); // генерируем оружие
                var slot = ship.Equipment.Weapons.GetSlot(i); // получаем копию слота
                slot.Installed = 1; // помечаем как установленное
                slot.Weapon = weapon; // записываем параметры
                ship.Equipment.Weapons.SetSlot(i, in slot); // сохраняем слот обратно
            }
        }

        public static byte GetWeaponSlotsCount(EShipType type, Fraction frac) // конфиг количества слотов по типу
        {
            switch (type) // минимальная логика по типу корабля
            {
                case EShipType.Fighter: return 2; // истребитель: 2 слота
                case EShipType.Transport: return 0; // транспорт: 0 слотов
                default: return 0; // по умолчанию: 0
            }
        }

        public static WeaponEntity GenerateWeapon(EShipType type, Fraction frac) // генерация параметров оружия
        {
            // Минимальные диапазоны; при желании варьируются по типу/фракции
            float damageMin = 10f, damageMax = 12f; // урон 10..12
            float rangeMin = 100f, rangeMax = 125f; // дальность 100..125

            if (type == EShipType.Fighter) // пример условной настройки
            {
                damageMin = 8f; damageMax = 14f; // бойцу шире разброс урона
                rangeMin = 90f; rangeMax = 130f; // и дальности
            }

            var weapon = new WeaponEntity
            {
                Damage = Random.Range(damageMin, damageMax), // случайный урон в диапазоне
                Range = Random.Range(rangeMin, rangeMax) // случайная дальность в диапазоне
            };
            return weapon; // возвращаем сущность оружия
        }
    }
}

