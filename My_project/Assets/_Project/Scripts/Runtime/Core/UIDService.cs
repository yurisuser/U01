using System.Collections.Generic;

namespace _Project.Scripts.Core
{
    /// <summary>Генерирует уникальные UID по типам сущностей.</summary>
    public static class UIDService
    {
        private static readonly Dictionary<EntityType, int> _counters = new(); // счётчики по типам

        /// <summary>Создаёт новый UID для указанного типа сущности.</summary>
        public static UID Create(EntityType type)
        {
            if (!_counters.TryGetValue(type, out int current))
                current = 0;
            current++;
            _counters[type] = current;
            return new Scripts.Core.UID(type, current);
        }
    }
}
