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
        private readonly SimulationEventBus _eventBus;
        private float _globalAccumulator;
        private ERunMode? _nextRunMode; // отложенное переключение режима после хода

        private ISimulationPipeline _globalPipeline;
        private ISimulationPipeline _localPipeline;

        public SimulationRootController(GameStateService gameState, SimulationClock clock) //конструктор
        {
            _gameState = gameState;
            _clock = clock;
            _eventBus = new SimulationEventBus();
            _globalPipeline = new NoopSimulationPipeline("GlobalNoop");
            _localPipeline = new _Project.Scripts.Simulation.Local.LocalSimulationPipeline();
        }

        /// <summary>Выполнить шаг из FixedUpdate с заданным fixedDeltaTime.</summary>
        public void TickFixed(float fixedDeltaTime) //Дергается из Bootstrap
        {
            _clock.SetDeltaTime(fixedDeltaTime);
            RunTick(fixedDeltaTime);
        }

        /// <summary>Общий исполняющий блок для шага.</summary>
        private void RunTick(float deltaTime)
        {
            _eventBus.Clear(); // новый буфер событий на шаг
            var mode = _gameState?.RunMode ?? ERunMode.Paused;

            if (mode != ERunMode.Paused)
            {
                if (CheckRunLocal())
                    RunLocal(deltaTime, mode);

                bool shouldRunGlobal = CheckRunGlobal(deltaTime);
                if (shouldRunGlobal)
                {
                    RunGlobal(mode);
                    if (mode == ERunMode.Step)
                        _nextRunMode = ERunMode.Paused; // шаг выполнен — обратно в паузу
                }
            }

            ApplyNextRunMode(mode);
        }

        private bool CheckRunLocal() // Локальная симуляция крутится только если есть выбранная система.
        {
            return _gameState?.GetSelectedSystem() != null;
        }

        private void RunLocal(float deltaTime, ERunMode mode)
        {
            var localCtx = new SimulationStepContext(_gameState, _clock.Day, deltaTime, mode, _eventBus);
            _localPipeline?.RunStep(in localCtx);
        }

        private bool CheckRunGlobal(float deltaTime)
        {
            _globalAccumulator += deltaTime;
            return _globalAccumulator >= SimulationConsts.GlobalStepSeconds;
        }

        private void RunGlobal(ERunMode mode)
        {
            _globalAccumulator -= SimulationConsts.GlobalStepSeconds;
            var day = _clock.NextDay();
            var globalCtx = new SimulationStepContext(_gameState, day, SimulationConsts.GlobalStepSeconds, mode, _eventBus);
            _globalPipeline?.RunStep(in globalCtx);
        }

        private void ApplyNextRunMode(ERunMode current)
        {
            if (!_nextRunMode.HasValue)
                return;

            var next = _nextRunMode.Value;
            _nextRunMode = null;

            if (current == next)
                return;

            _gameState?.SetRunMode(next);
            Debug.Log($"[Simulation] RunMode: {current} -> {next}");
        }
    }
}
