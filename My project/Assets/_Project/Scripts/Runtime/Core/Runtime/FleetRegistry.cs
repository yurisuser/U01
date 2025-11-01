using _Project.Scripts.Core;
using _Project.Scripts.Ships;

namespace _Project.Scripts.Core.Runtime
{
    public sealed class FleetRegistry
    {
        private readonly SystemRegistry _systems;
        private readonly OwnershipMap _ownership;

        public FleetRegistry(SystemRegistry systems, OwnershipMap ownership)
        {
            _systems = systems;
            _ownership = ownership;
        }

        public void Reset()
        {
            // Здесь ничего не очищаем напрямую — SystemRegistry и OwnershipMap уже сброшены контекстом.
        }

        public int RegisterShip(int systemId, in Ship ship)
        {
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
            // Заглушка — логику движения/приказов добавим позже.
        }
    }
}
