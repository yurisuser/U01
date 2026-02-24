using _Project.Scripts.Const;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Simulation.Continuum;
using _Project.Scripts.Simulation.Global.Debug;
using UnityEngine;

namespace _Project.Scripts.Simulation.Core
{
    /// <summary>Оркестратор симуляции: дергает пайплайны по режиму выполнения.</summary>
    public sealed class SimulationRootController : System.IDisposable
    {
        private readonly GameStateService _gameState; // Глобальное состояние игры (режим, галактика, selected system).
        private readonly SimulationClock _clock; // Счетчик дней и локального deltaTime.
        private readonly SimulationEventBus _eventBus; // Буфер событий кадра для main-thread стадий.
        private readonly ContinuumService _continuumService; // Сервис транзитов между системами.
        private readonly SimulationGlobalWorker _globalWorker; // Worker, который крутит глобальные ходы вне main thread.
        private float _globalAccumulator; // Накопитель времени до следующего глобального хода.
        private ERunMode? _nextRunMode; // Отложенное переключение режима после завершения тика.

        private ISimulationPipeline _globalPipeline; // Глобальный staged pipeline (выполняется в worker).
        private ISimulationPipeline _localPipeline; // Локальный pipeline активной системы (main thread).

        public SimulationRootController(GameStateService gameState, SimulationClock clock) // Конструктор оркестратора симуляции.
        {
            _gameState = gameState;
            _clock = clock;
            _gameState?.ApplyCurrentTurnNumber(_clock.Day); // Синхронизируем стартовый номер хода для UI.
            _eventBus = new SimulationEventBus(); // Main-thread event bus.
            _continuumService = new ContinuumService(); // сервис Continuum для глобальных прыжков
            _globalPipeline = new _Project.Scripts.Simulation.Global.GlobalSimulationPipeline();
            _localPipeline = new _Project.Scripts.Simulation.Local.LocalSimulationPipeline();
            _globalWorker = new SimulationGlobalWorker(_gameState, _globalPipeline, _continuumService);
            _globalWorker.Start(); // Поднимаем поток глобальной симуляции один раз.
            _gameState?.SetGlobalIdleWaiter(() => _globalWorker.WaitForIdle(3000)); // Handshake: выбор системы ждёт, пока глобальный worker станет idle.
        }

        /// <summary>Выполнить шаг из FixedUpdate с заданным fixedDeltaTime.</summary>
        public void TickFixed(float fixedDeltaTime) // Вызывается из Bootstrap в FixedUpdate.
        {
            _clock.SetDeltaTime(fixedDeltaTime); // Сохраняем dt текущего fixed-кадра.
            RunTick(fixedDeltaTime); // Запускаем общий цикл локал+глобал.
        }

        /// <summary>Общий исполняющий блок для шага.</summary>
        private void RunTick(float deltaTime)
        {
            _eventBus.Clear(); // Новый буфер событий на текущий шаг.
            var mode = _gameState?.RunMode ?? ERunMode.Paused; // Если gameState null, считаем симуляцию на паузе.

            if (mode != ERunMode.Paused)
            {
                if (CheckRunLocal())
                    RunLocal(deltaTime, mode); // Локал всегда на main thread.

                bool shouldRunGlobal = CheckRunGlobal(deltaTime);
                if (shouldRunGlobal)
                {
                    RunGlobal(mode); // Глобал отправляется в worker.
                    if (mode == ERunMode.Step)
                        _nextRunMode = ERunMode.Paused; // Одноразовый Step завершился, надо вернуть Paused.
                }
            }

            ApplyNextRunMode(mode); // Применяем отложенные переключения режима.
        }

        private bool CheckRunLocal() // Локальная симуляция крутится только если в SystemMap активирована локальная система.
        {
            return _gameState?.GetActiveLocalSystem() != null; // null => локал пропускаем.
        }

        private void RunLocal(float deltaTime, ERunMode mode)
        {
            int activeSystemIndex = _gameState?.ActiveLocalSystemIndex ?? -1; // Снимок реально активной локальной системы.
            var localCtx = new SimulationStepContext(_gameState, _clock.Day, deltaTime, mode, _eventBus, activeSystemIndex); // Контекст локального тика.
            _localPipeline?.RunStep(in localCtx); // Локальный конвейер.
        }

        private bool CheckRunGlobal(float deltaTime)
        {
            _globalAccumulator += deltaTime; // Накопили время с прошлого fixed-кадра.
            return _globalAccumulator >= SimulationConsts.GlobalStepSeconds; // Если накопили интервал глобального хода, ставим задачу в worker.
        }

        private void RunGlobal(ERunMode mode)
        {
            _globalAccumulator -= SimulationConsts.GlobalStepSeconds; // Потребили один слот глобального шага.
            var day = _clock.NextDay(); // Инкремент игрового дня привязан к global tick.
            _gameState?.ApplyCurrentTurnNumber(day); // Прокидываем текущий номер хода в game state для UI.
            int activeSystemIndex = _gameState?.ActiveLocalSystemIndex ?? -1; // Снимок реально активной локальной системы.
            GlobalSyncDebugLog.Log("Root", "run-global day=" + day + " mode=" + mode + " active=" + activeSystemIndex);
            _globalWorker.EnqueueRunStep(day, mode, activeSystemIndex); // Фиксируем параметры шага на границе тика и отдаем в очередь worker.
        }

        private void ApplyNextRunMode(ERunMode current)
        {
            if (!_nextRunMode.HasValue)
                return; // Нет отложенного переключения.

            var next = _nextRunMode.Value;
            _nextRunMode = null; // Сбрасываем pending-состояние сразу.

            if (current == next)
                return; // Нечего менять.

            _gameState?.SetRunMode(next); // Применяем режим в game state.
            Debug.Log($"[Simulation] RunMode: {current} -> {next}"); // Лог для диагностики pause/step/auto.
        }

        public void Dispose()
        {
            _globalWorker?.Dispose(); // Корректно останавливаем поток global worker.
        }
    }
}
