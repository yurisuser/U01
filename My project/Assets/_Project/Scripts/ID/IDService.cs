using System.Collections.Generic;
using _Project.Scripts.Core;

namespace _Project.Scripts.ID
{
    public static class IDService
    {
        private static readonly Dictionary<EntityType, int> _counters = new(); // счётчики по типам

        public static UID Create(EntityType type)
        {
            if (!_counters.TryGetValue(type, out int current))
                current = 0;
            current++;
            _counters[type] = current;
            return new UID(type, current);
        }
    }
}