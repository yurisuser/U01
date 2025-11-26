namespace _Project.Scripts.Ships
{
    public readonly struct RaiderShip : IShipTemplate // шаблон рейдера
    {
        private const int Hp = 90;           // здоровье
        private const float MaxSpeed = 36f;  // максимальная скорость
        private const float Agility = 0.14f; // маневренность
        private const byte WeaponSlots = 3;  // число слотов

        public ShipStats GetStats() // вернуть характеристики
            => new ShipStats { Hp = Hp, MaxSpeed = MaxSpeed, Agility = Agility }; // собираем структуру

        public byte GetWeaponSlots() // вернуть число слотов
            => WeaponSlots; // константа
    }
}

