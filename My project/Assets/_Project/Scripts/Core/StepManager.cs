using System;

namespace _Project.Scripts.Core
{
    public sealed class StepManager
    {
        private readonly Action<int, float> _pipeline; // внешний пайплайн (может быть no-op)
        private readonly float _stepDurationSeconds;   // длительность логического шага (обычно 2.0 c)
        private float _accum;                          // аккумулятор прошедшего времени
        private int _tick;                             // счётчик логических тиков

        public StepManager(float stepDurationSeconds, Action<int, float> pipeline)
        {
            _stepDurationSeconds = stepDurationSeconds > 0f ? stepDurationSeconds : 2f;
            _pipeline = pipeline ?? ((_, __) => { });
        }

        // Core теперь просто передаёт сюда deltaTime каждый кадр
        public void Update(float dt)
        {
            _accum += dt;
            while (_accum >= _stepDurationSeconds)
            {
                _accum -= _stepDurationSeconds;
                _tick++;
                Step(_tick, _stepDurationSeconds);
            }
        }

        public void Step(int tickIndex, float dt)
        {
            _pipeline(tickIndex, dt);
        }
    }
}