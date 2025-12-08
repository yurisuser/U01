using _Project.Scripts.Core;
using UnityEngine;

namespace _Project.Scripts.Simulation.Core
{
    /// <summary>Событие симуляции за ход.</summary>
    public readonly struct SimulationEvent
    {
        public SimulationEvent(
            SimulationEventType type,
            int systemIndex,
            int day,
            UID subject,
            UID target = default,
            Vector3 position = default,
            float value = 0f)
        {
            Type = type;
            SystemIndex = systemIndex;
            Day = day;
            Subject = subject;
            Target = target;
            Position = position;
            Value = value;
        }

        public SimulationEventType Type { get; }
        public int SystemIndex { get; }   // индекс системы, где произошло
        public int Day { get; }           // ход/день симуляции
        public UID Subject { get; }       // главный участник события
        public UID Target { get; }        // второй участник (цель) при необходимости
        public Vector3 Position { get; }  // позиция (если применимо)
        public float Value { get; }       // числовой payload (например, урон)
    }
}
