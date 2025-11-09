namespace _Project.Scripts.Ships
{
    public static class ShipConfigGenerator // временный генератор конфигурации корабля
    {
        public enum TemplateId : byte // доступные шаблоны
        {
            Default = 0, // базовый
            Raider = 1,  // рейдер
            Transport = 2 // транспорт
        }

        public static ShipConfig CreateDefault() => Create(TemplateId.Default); // совместимость

        public static ShipConfig Create(TemplateId id) // собрать конфигурацию по ID шаблона
        {
            switch (id)
            {
                case TemplateId.Raider:
                    return CreateRaider(); // конфигурация рейдера
                case TemplateId.Transport:
                    return CreateTransport(); // конфигурация транспорта
                default:
                    return CreateDefaultInternal(); // базовая конфигурация
            }
        }

        private static ShipConfig CreateDefaultInternal() // DefaultShip
        {
            var template = new DefaultShip(); // шаблон default
            return new ShipConfig { Stats = template.GetStats(), WeaponSlotsCount = template.GetWeaponSlots() }; // собираем
        }

        private static ShipConfig CreateRaider() // RaiderShip
        {
            var template = new RaiderShip(); // шаблон raider
            return new ShipConfig { Stats = template.GetStats(), WeaponSlotsCount = template.GetWeaponSlots() }; // собираем
        }

        private static ShipConfig CreateTransport() // TransportShip
        {
            var template = new TransportShip(); // шаблон transport
            return new ShipConfig { Stats = template.GetStats(), WeaponSlotsCount = template.GetWeaponSlots() }; // собираем
        }
    }
}
