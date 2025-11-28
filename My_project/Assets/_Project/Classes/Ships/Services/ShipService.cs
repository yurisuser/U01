namespace _Project.Scripts.Ships
{
    /// <summary>Минимальный сервис для кораблей: прямой спавн через ShipCreator.</summary>
    public static class ShipService
    {
        /// <summary>Создать корабль по заявке. Добавление в мир делает вызывающий.</summary>
        public static Ship Spawn(ShipSpawnRequest request)
        {
            // TODO: учесть Position/Rotation из заявки (ShipCreator пока ставит свои значения).
            // TODO: добавить корабль в SystemState и оповестить EventBus (ShipSpawned).
            var ship = ShipCreator.CreateShip(request.Fraction, request.PilotUid);
            return ship;
        }
    }
}
