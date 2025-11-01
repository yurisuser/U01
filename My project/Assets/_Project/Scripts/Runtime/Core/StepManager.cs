using System;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Simulation;

namespace _Project.Scripts.Core
{
    public sealed class StepManager
    {
        private readonly GameStateService _state;
        private readonly Executor _executor;

        private float _accum;

        public StepManager(GameStateService state, Executor executor)
        {
            _state    = state ?? throw new ArgumentNullException(nameof(state));
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _accum    = 0f;
        }

        public void Update(float dt)
        {
            if (dt < 0f)
                dt = 0f;

            var snapshot     = _state.Current;
            var stepDuration = GetStepDuration(snapshot);

            if (snapshot.RequestStep)
            {
                ExecuteStep(stepDuration);
                snapshot     = _state.Current;
                stepDuration = GetStepDuration(snapshot);
            }

            if (snapshot.RunMode != ERunMode.Auto)
            {
                _accum = 0f;
                _state.SetStepProgress(0f);
                return;
            }

            stepDuration = Math.Max(0.0001f, stepDuration);
            _accum += dt;
            _state.SetStepProgress(Clamp01(_accum / stepDuration));

            const int safetyCap = 1000;
            int loops = 0;

            while (_accum >= stepDuration && loops++ < safetyCap)
            {
                _accum -= stepDuration;
                ExecuteStep(stepDuration);

                snapshot     = _state.Current;
                stepDuration = Math.Max(0.0001f, GetStepDuration(snapshot));
                _state.SetStepProgress(Clamp01(_accum / stepDuration));
            }

            if (loops >= safetyCap)
            {
                _accum = 0f;
                _state.SetStepProgress(0f);
            }
            else
            {
                _state.SetStepProgress(Clamp01(_accum / stepDuration));
            }
        }

        private void ExecuteStep(float dt)
        {
            var snapshot = _state.Current;
            var next     = snapshot;

            _executor.Execute(ref next, dt);
            next.TickIndex++;
            next.RequestStep = false;
            _state.SetStepProgress(0f);

            _state.Commit(next);
        }

        private static float GetStepDuration(in GameStateService.Snapshot snapshot)
        {
            var baseSeconds = snapshot.LogicStepSeconds > 0f ? snapshot.LogicStepSeconds : 0.0001f;
            var speedMul    = Math.Max(1, (int)snapshot.PlayStepSpeed);
            return baseSeconds / speedMul;
        }

        private static float Clamp01(float value)
        {
            if (value < 0f) return 0f;
            if (value > 1f) return 1f;
            return value;
        }
    }
}
