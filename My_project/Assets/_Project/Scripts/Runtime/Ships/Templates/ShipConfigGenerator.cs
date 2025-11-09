namespace _Project.Scripts.Ships
{
    public static class ShipConfigGenerator // временный генератор конфигурации корабля
    {
        public static ShipConfig CreateDefault() // собрать конфигурацию по шаблону DefaultShip
        {
            var template = new DefaultShip(); // используем текущий шаблон
            return new ShipConfig
            {
                Stats = template.GetStats(), // базовые параметры
                WeaponSlotsCount = template.GetWeaponSlots() // количество слотов
            };
        }
    }
}

