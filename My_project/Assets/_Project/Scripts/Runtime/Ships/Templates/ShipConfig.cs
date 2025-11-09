namespace _Project.Scripts.Ships
{
    public struct ShipConfig // временная конфигурация корабля (только числа)
    {
        public ShipStats Stats; // базовые параметры корабля
        public byte WeaponSlotsCount; // количество оружейных слотов (0..8)
    }
}

