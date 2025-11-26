using System.Collections.Generic;
using _Project.Scripts.Core;

namespace _Project.Scripts.Core.Runtime
{
    /// <summary>Карта принадлежности UID к системе и локальному слоту.</summary>
    public sealed class OwnershipMap
    {
        private readonly Dictionary<UID, EntityLocation> _locations = new Dictionary<UID, EntityLocation>(256); // Быстрый доступ к локациям.

        /// <summary>Полностью очищает карту владения.</summary>
        public void Reset()
        {
            _locations.Clear();
        }

        /// <summary>Регистрирует или обновляет местоположение сущности.</summary>
        public void Register(UID uid, in EntityLocation location)
        {
            _locations[uid] = location;
        }

        /// <summary>Удаляет запись о сущности.</summary>
        public void Unregister(UID uid)
        {
            _locations.Remove(uid);
        }

        /// <summary>Пробует получить местоположение сущности.</summary>
        public bool TryGetLocation(UID uid, out EntityLocation location)
        {
            return _locations.TryGetValue(uid, out location);
        }

        /// <summary>Обновляет существующую запись, если UID зарегистрирован.</summary>
        public void UpdateLocation(UID uid, in EntityLocation location)
        {
            if (_locations.ContainsKey(uid))
                _locations[uid] = location;
        }
    }

    /// <summary>Координаты сущности внутри симуляции (система + слот).</summary>
    public struct EntityLocation
    {
        public int SystemId; // В какой системе.
        public int Slot; // Какой слот внутри системы.

        /// <summary>Создаёт локацию по системе и слоту.</summary>
        public EntityLocation(int systemId, int slot)
        {
            SystemId = systemId;
            Slot = slot;
        }
    }
}
