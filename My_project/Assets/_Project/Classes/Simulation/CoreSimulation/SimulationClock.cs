using System;

namespace _Project.Scripts.Simulation.Core
{
    /// <summary>Счётчик ходов (дней) и базовый шаг времени.</summary>
    public sealed class SimulationClock
    {
        private int _day;
        private float _deltaTime;

        public SimulationClock(float deltaTime)
        {
            _deltaTime = Math.Max(0.0001f, deltaTime);
            _day = 0;
        }

        /// <summary>Текущий номер хода/дня.</summary>
        public int Day => _day;
        /// <summary>Обновить базовый dt.</summary>
        public void SetDeltaTime(float deltaTime)
        {
            _deltaTime = Math.Max(0.0001f, deltaTime);
        }

        /// <summary>Перейти к следующему ходу/дню и вернуть его номер.</summary>
        public int NextDay()
        {
            _day++;
            return _day;
        }
    }
}
