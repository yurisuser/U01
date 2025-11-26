namespace _Project.Scripts.Ships
{
    public readonly struct TransportShip : IShipTemplate // шаблон транспорта
    {
        private const int Hp = 160;         // здоровье (повышенное)
        private const float MaxSpeed = 18f; // максимальная скорость (ниже)
        private const float Agility = 0.05f;// маневренность (ниже)
        private const byte WeaponSlots = 1; // один защитный слот

        public ShipStats GetStats() // вернуть характеристики
            => new ShipStats { Hp = Hp, MaxSpeed = MaxSpeed, Agility = Agility }; // собираем структуру

        public byte GetWeaponSlots() // вернуть число слотов
            => WeaponSlots; // константа
    }
}

