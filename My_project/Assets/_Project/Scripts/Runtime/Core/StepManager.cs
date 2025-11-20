using System;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Simulation;

namespace _Project.Scripts.Core
{
    /// <summary>Управляет шагами симуляции: планирование, прогресс, синхронизация с UI.</summary>
    public sealed class StepManager
    {
        private readonly GameStateService _state;
        private readonly SimulationThread _simulationThread;

        private float _accum;
        private float _visualTime;
        private bool _stepInFlight;

        /// <summary>Создаёт менеджер шагов с сервисом состояния и потоком симуляции.</summary>
        public StepManager(GameStateService state, SimulationThread simulationThread)
        {
            _state            = state ?? throw new ArgumentNullException(nameof(state));
            _simulationThread = simulationThread ?? throw new ArgumentNullException(nameof(simulationThread));
            _accum            = 0f;
            _visualTime      = 0f;
            _stepInFlight     = false;
        }

        /// <summary>Обновляет прогресс шага, планирует и принимает выполнение тикoв.</summary>
        public void Update(float dt)
        {
            if (dt < 0f)
                dt = 0f;

            TryConsumeCompletedStep();

            var snapshot     = _state.Current;
            var stepDuration = Math.Max(0.0001f, GetStepDuration(snapshot));

            if (snapshot.RequestStep)
            {
                if (!_stepInFlight && TryScheduleStep(snapshot, stepDuration))
                {
                    _accum = 0f;
                    _visualTime = 0f;
                    _state.SetStepProgress(0f);
                }
                return;
            }

            bool allowVisualProgress = snapshot.RunMode == ERunMode.Auto || _stepInFlight; // визуал может идти до конца шага
            bool allowNewSteps = snapshot.RunMode == ERunMode.Auto; // новые шаги только в авто-режиме

            if (!allowVisualProgress)
            {
                _visualTime = Math.Min(_visualTime + dt, stepDuration); // доигрываем текущий шаг
                _accum = _visualTime >= stepDuration ? stepDuration : 0f; // если шаг завершён — готовы запустить следующий
                _state.SetStepProgress(stepDuration > 0f ? Clamp01(_visualTime / stepDuration) : 1f);
                return;
            }

            if (allowNewSteps)
            {
                _accum += dt;

                if (_accum >= stepDuration && !_stepInFlight)
                {
                    if (TryScheduleStep(snapshot, stepDuration))
                    {
                        _accum -= stepDuration;
                    }
                }
            }

            _visualTime += dt;

            if (allowNewSteps)
            {
                while (_visualTime >= stepDuration)
                {
                    if (_state.TryPromoteNextShips())
                    {
                        _visualTime -= stepDuration;
                        if (_visualTime < 0f)
                            _visualTime = 0f;

                        if (!_stepInFlight && _accum >= stepDuration)
                        {
                            if (TryScheduleStep(snapshot, stepDuration))
                                _accum -= stepDuration;
                        }
                    }
                    else
                    {
                        _visualTime = stepDuration;
                        break;
                    }
                }
            }
            else if (_visualTime > stepDuration)
            {
                _visualTime = stepDuration; // фиксируемся на конце шага, пока пауза
            }

            float progress = stepDuration > 0f ? Clamp01(_visualTime / stepDuration) : 1f;
            _state.SetStepProgress(progress);
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
