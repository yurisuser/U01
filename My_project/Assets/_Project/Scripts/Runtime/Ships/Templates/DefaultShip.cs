namespace _Project.Scripts.Ships
{
    public readonly struct DefaultShip : IShipTemplate // реализация шаблона Default
    {
        // Базовые статы по умолчанию
        private const int Hp = 100; // здоровье
        private const float MaxSpeed = 30f; // максимальная скорость
        private const float Agility = 0.1f; // маневренность
        private const byte WeaponSlots = 3; // число слотов

        public ShipStats GetStats() // вернуть базовые характеристики
            => new ShipStats { Hp = Hp, MaxSpeed = MaxSpeed, Agility = Agility }; // заполнение статов

        public byte GetWeaponSlots() // вернуть число оружейных слотов
            => WeaponSlots; 
    }
}
