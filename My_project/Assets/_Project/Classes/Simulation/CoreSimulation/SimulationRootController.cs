using _Project.Scripts.Const;
using _Project.Scripts.Core.GameState;
using UnityEngine;

namespace _Project.Scripts.Simulation.Core
{
    /// <summary>Оркестратор симуляции: дергает пайплайны по режиму выполнения.</summary>
    public sealed class SimulationRootController
    {
        private readonly GameStateService _gameState;
        private readonly SimulationClock _clock;
        private float _globalAccumulator;
        private ERunMode? _nextRunMode; // отложенное переключение режима после хода

        private ISimulationPipeline _globalPipeline;
        private ISimulationPipeline _localPipeline;

        public SimulationRootController(GameStateService gameState, SimulationClock clock) //конструктор
        {
            _gameState = gameState;
            _clock = clock;
            _globalPipeline = new NoopSimulationPipeline("GlobalNoop");
            _localPipeline = new NoopSimulationPipeline("LocalNoop");
        }

        /// <summary>Выполнить шаг симуляции согласно текущему режиму (dt берётся из SimulationClock).</summary>
        public void Tick()
        {
            RunTick(_clock.DeltaTime);
        }

        /// <summary>Выполнить шаг из FixedUpdate с заданным fixedDeltaTime.</summary>
        public void TickFixed(float fixedDeltaTime)
        {
            _clock.SetDeltaTime(fixedDeltaTime);
            RunTick(fixedDeltaTime);
        }

        /// <summary>Общий исполняющий блок для шага.</summary>
        private void RunTick(float deltaTime)
        {
            var mode = _gameState?.RunMode ?? ERunMode.Paused;

            // Локальная часть крутится каждый фикс-кадр, если не пауза.
            if (mode != ERunMode.Paused)
            {
                var localCtx = new SimulationStepContext(_gameState, _clock.Day, deltaTime, mode);
                _localPipeline?.RunStep(localCtx);

                // Глобальная часть — раз в заданный интервал (1 ход = 1 день).
                _globalAccumulator += deltaTime;
                if (_globalAccumulator >= SimulationConsts.GlobalStepSeconds)
                {
                    _globalAccumulator -= SimulationConsts.GlobalStepSeconds;
                    var day = _clock.AdvanceDay();
                    var globalCtx = new SimulationStepContext(_gameState, day, SimulationConsts.GlobalStepSeconds, mode);
                    Debug.Log($"[Simulation] Day={day}, mode={mode}, dt={SimulationConsts.GlobalStepSeconds:0.###}");
                    _globalPipeline?.RunStep(globalCtx);

                    if (mode == ERunMode.Step)
                        _nextRunMode = ERunMode.Paused; // шаг выполнен — обратно в паузу
                }
            }

            // Применяем отложенную смену режима после хода.
            if (_nextRunMode.HasValue)
            {
                var current = mode;
                var next = _nextRunMode.Value;
                _nextRunMode = null;

                if (current != next)
                {
                    _gameState?.SetRunMode(next);
                    Debug.Log($"[Simulation] RunMode: {current} -> {next}");
                }
            }
        }

        /// <summary>Поставить симуляцию на паузу.</summary>
        public void Pause() => _nextRunMode = ERunMode.Paused;

        /// <summary>Включить автоматический режим (поточный, шаг за шагом без паузы).</summary>
        public void SetAuto() => _nextRunMode = ERunMode.Auto;

        /// <summary>Сделать один шаг и вернуться в паузу.</summary>
        public void StepOnce() => _nextRunMode = ERunMode.Step;
    }
}
