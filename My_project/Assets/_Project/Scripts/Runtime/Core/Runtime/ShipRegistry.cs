using _Project.Scripts.Core;
using _Project.Scripts.Ships;

namespace _Project.Scripts.Core.Runtime
{
    /// <summary>
    /// Простая база одиночных кораблей. Здесь мы знаем, в какой системе стоит каждый корабль,
    /// и по UID можно быстро получить его позицию или переставить в другую систему.
    /// Отдельных "флотов" пока нет, поэтому класс так и называется — ShipRegistry.
    /// </summary>
    public sealed class ShipRegistry
    {
        private readonly SystemRegistry _systems;
        private readonly OwnershipMap _ownership;

        public ShipRegistry(SystemRegistry systems, OwnershipMap ownership)
        {
            _systems = systems;
            _ownership = ownership;
        }

        public void Reset()
        {
            // Нам не нужно чистить что-то вручную. SystemRegistry и OwnershipMap
            // сбрасываются через свой Reset(), и этого хватает.
        }

        public int RegisterShip(int systemId, in Ship ship)
        {
            // Добавляем корабль в нужную систему и запоминаем,
            // где он стоит (systemId + номер слота).
            var slot = _systems.AddShip(systemId, in ship);
            _ownership.Register(ship.Uid, new EntityLocation(systemId, slot));
            return slot;
        }

        public bool TryGetShip(UID uid, out Ship ship, out EntityLocation location)
        {
            ship = default;
            location = default;

            if (!_ownership.TryGetLocation(uid, out location))
                return false;

            if (!_systems.TryGetShip(location.SystemId, location.Slot, out ship))
                return false;

            return true;
        }

        public bool TryMoveShip(UID uid, int targetSystemId)
        {
            if (!_ownership.TryGetLocation(uid, out var location))
                return false;

            if (!_systems.TryRemoveShip(location.SystemId, location.Slot, out var ship))
                return false;

            var newSlot = _systems.AddShip(targetSystemId, ship);
            _ownership.UpdateLocation(uid, new EntityLocation(targetSystemId, newSlot));
            return true;
        }

        public bool TryRemoveShip(UID uid)
        {
            if (!_ownership.TryGetLocation(uid, out var location))
                return false;

            if (!_systems.TryRemoveShip(location.SystemId, location.Slot, out var ship))
                return false;

            _ownership.Unregister(ship.Uid);
            return true;
        }

        public void Tick(float dt)
        {
            // Пока движение кораблей будет реализовано позже, поэтому здесь пусто.
            // Позже сюда можно добавить обновление приказов или автоматическое перемещение.
        }
    }
}
