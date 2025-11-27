using System;

namespace _Project.Scripts.Simulation.Core
{
    /// <summary>Счётчик ходов и базовый шаг времени.</summary>
    public sealed class SimulationClock
    {
        private int _tick;
        private float _deltaTime;

        public SimulationClock(float deltaTime)
        {
            _deltaTime = Math.Max(0.0001f, deltaTime);
            _tick = 0;
        }

        /// <summary>Текущий номер хода.</summary>
        public int Tick => _tick;

        /// <summary>Базовый dt для локальной симуляции.</summary>
        public float DeltaTime => _deltaTime;

        /// <summary>Обновить базовый dt.</summary>
        public void SetDeltaTime(float deltaTime)
        {
            _deltaTime = Math.Max(0.0001f, deltaTime);
        }

        /// <summary>Перейти к следующему ходу и вернуть его номер.</summary>
        public int AdvanceTick()
        {
            _tick++;
            return _tick;
        }

        /// <summary>Сбросить тик на указанное значение.</summary>
        public void Reset(int tick = 0)
        {
            _tick = Math.Max(0, tick);
        }
    }
}
