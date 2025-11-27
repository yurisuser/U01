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

        private ISimulationPipeline _globalPipeline;
        private ISimulationPipeline _localPipeline;

        public SimulationRootController(GameStateService gameState, SimulationClock clock)
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
            if (mode == ERunMode.Paused)
                return;

            var tick = _clock.AdvanceTick();
            var ctx = new SimulationStepContext(_gameState, tick, deltaTime, mode);

            Debug.Log($"[Simulation] Tick={tick}, mode={mode}, dt={deltaTime:0.###}");

            _globalPipeline?.RunStep(ctx);
            _localPipeline?.RunStep(ctx);

            // В пошаговом режиме сразу возвращаемся в паузу после одного шага.
            if (mode == ERunMode.Step)
                _gameState?.SetRunMode(ERunMode.Paused);
        }

        /// <summary>Поставить симуляцию на паузу.</summary>
        public void Pause() => _gameState?.SetRunMode(ERunMode.Paused);

        /// <summary>Включить автоматический режим (поточный, шаг за шагом без паузы).</summary>
        public void SetAuto() => _gameState?.SetRunMode(ERunMode.Auto);

        /// <summary>Сделать один шаг и вернуться в паузу.</summary>
        public void StepOnce()
        {
            _gameState?.SetRunMode(ERunMode.Step);
            Tick();
        }
    }
}
