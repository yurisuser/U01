using System;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Simulation;

namespace _Project.Scripts.Core
{
    public sealed class StepManager
    {
        private readonly GameStateService _state;
        private readonly SimulationThread _simulationThread;

        private float _accum;
        private bool _stepInFlight;

        public StepManager(GameStateService state, SimulationThread simulationThread)
        {
            _state            = state ?? throw new ArgumentNullException(nameof(state));
            _simulationThread = simulationThread ?? throw new ArgumentNullException(nameof(simulationThread));
            _accum            = 0f;
            _stepInFlight     = false;
        }

        public void Update(float dt)
        {
            if (dt < 0f)
                dt = 0f;

            TryConsumeCompletedStep();

            var snapshot     = _state.Current;
            var stepDuration = GetStepDuration(snapshot);

            if (snapshot.RequestStep)
            {
                if (!_stepInFlight && TryScheduleStep(snapshot, Math.Max(0.0001f, stepDuration)))
                {
                    _accum = 0f;
                    _state.SetStepProgress(0f);
                }
                return;
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

            if (_accum >= stepDuration && !_stepInFlight)
            {
                if (TryScheduleStep(snapshot, stepDuration))
                {
                    _accum -= stepDuration;
                    _state.SetStepProgress(Clamp01(_accum / stepDuration));
                }
            }
        }

        private void TryConsumeCompletedStep()
        {
            if (_simulationThread == null)
                return;

            if (_simulationThread.TryGetCompletedStep(out var completedSnapshot))
            {
                _stepInFlight = false;

                var latest = _state.Current;
                completedSnapshot.RunMode             = latest.RunMode;
                completedSnapshot.PlayStepSpeed       = latest.PlayStepSpeed;
                completedSnapshot.LogicStepSeconds    = latest.LogicStepSeconds;
                completedSnapshot.Galaxy              = latest.Galaxy;
                completedSnapshot.SelectedSystemIndex = latest.SelectedSystemIndex;
                completedSnapshot.TickIndex           = latest.TickIndex + 1;
                completedSnapshot.RequestStep         = false;

                _state.Commit(completedSnapshot);

                var refreshed = _state.Current;
                var duration  = Math.Max(0.0001f, GetStepDuration(refreshed));
                _state.SetStepProgress(Clamp01(_accum / duration));
            }
        }

        private bool TryScheduleStep(in GameStateService.Snapshot snapshot, float dt)
        {
            if (_simulationThread == null)
                return false;

            var stepSnapshot = snapshot;
            stepSnapshot.RequestStep = false;

            if (!_simulationThread.TryScheduleStep(stepSnapshot, dt))
                return false;

            _stepInFlight = true;
            return true;
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
