using System.Collections.Generic;

namespace _Project.Scripts.Simulation.Core
{
    /// <summary>Простой буфер событий симуляции за ход.</summary>
    public sealed class SimulationEventBus
    {
        private readonly List<SimulationEvent> _events = new List<SimulationEvent>(32);

        public IReadOnlyList<SimulationEvent> Events => _events;

        /// <summary>Добавить событие в буфер.</summary>
        public void Add(in SimulationEvent evt)
        {
            _events.Add(evt);
        }

        /// <summary>Очистить буфер (вызывается в конце хода).</summary>
        public void Clear()
        {
            _events.Clear();
        }

        /// <summary>Убедиться, что буфер сможет вместить минимум указанное число записей.</summary>
        public void EnsureCapacity(int capacity)
        {
            if (capacity > _events.Capacity)
                _events.Capacity = capacity;
        }
    }
}
