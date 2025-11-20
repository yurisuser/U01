using System.Collections.Generic;
using _Project.Scripts.Core;

namespace _Project.Scripts.Core.Runtime
{
    // Карта принадлежности UID к системе и локальному слоту.
    public sealed class OwnershipMap
    {
        private readonly Dictionary<UID, EntityLocation> _locations = new Dictionary<UID, EntityLocation>(256); // Быстрый доступ к локациям.

        // Полностью очищаем карту.
        public void Reset()
        {
            _locations.Clear();
        }

        // Регистрируем или обновляем местоположение сущности.
        public void Register(UID uid, in EntityLocation location)
        {
            _locations[uid] = location;
        }

        // Удаляем запись о сущности.
        public void Unregister(UID uid)
        {
            _locations.Remove(uid);
        }

        // Узнаём, где находится сущность.
        public bool TryGetLocation(UID uid, out EntityLocation location)
        {
            return _locations.TryGetValue(uid, out location);
        }

        // Обновляем существующую запись.
        public void UpdateLocation(UID uid, in EntityLocation location)
        {
            if (_locations.ContainsKey(uid))
                _locations[uid] = location;
        }
    }

    // Координаты сущности внутри симуляции.
    public struct EntityLocation
    {
        public int SystemId; // В какой системе.
        public int Slot; // Какой слот внутри системы.

        public EntityLocation(int systemId, int slot)
        {
            SystemId = systemId;
            Slot = slot;
        }
    }
}
