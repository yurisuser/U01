using System.Collections.Generic;
using _Project.Scripts.Core;

namespace _Project.Scripts.Core.Runtime
{
    /// <summary>
    /// Карта принадлежности UID к системе и локальному слоту.
    /// </summary>
    public sealed class OwnershipMap
    {
        private readonly Dictionary<UID, EntityLocation> _locations = new Dictionary<UID, EntityLocation>(256);

        public void Reset()
        {
            _locations.Clear();
        }

        public void Register(UID uid, in EntityLocation location)
        {
            _locations[uid] = location;
        }

        public void Unregister(UID uid)
        {
            _locations.Remove(uid);
        }

        public bool TryGetLocation(UID uid, out EntityLocation location)
        {
            return _locations.TryGetValue(uid, out location);
        }

        public void UpdateLocation(UID uid, in EntityLocation location)
        {
            if (_locations.ContainsKey(uid))
                _locations[uid] = location;
        }
    }

    public struct EntityLocation
    {
        public int SystemId;
        public int Slot;

        public EntityLocation(int systemId, int slot)
        {
            SystemId = systemId;
            Slot = slot;
        }
    }
}
