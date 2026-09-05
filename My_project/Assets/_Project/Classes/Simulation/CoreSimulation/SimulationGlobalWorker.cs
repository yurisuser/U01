using System;
using System.Collections.Generic;
using System.Threading;
using _Project.Scripts.Const;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Simulation.Continuum;
using _Project.Scripts.Simulation.Global.Debug;

namespace _Project.Scripts.Simulation.Core
{
    /// <summary>Отдельный поток глобальной симуляции: принимает команды хода и выполняет pipeline вне main thread.</summary>
    public sealed class SimulationGlobalWorker : IDisposable
    {
        private readonly object _queueLock = new object(); // Мьютекс на очередь задач worker-потока.
        private readonly Queue<WorkItem> _queue = new Queue<WorkItem>(8); // Очередь глобальных шагов, поставленных с main thread.
        private readonly AutoResetEvent _signal = new AutoResetEvent(false); // Будит поток, когда в очереди появилась новая задача.
        private readonly ManualResetEventSlim _idle = new ManualResetEventSlim(true); // Сигнал "worker простаивает" для handshake выбора системы.

        private readonly GameStateService _gameState; // Общее состояние игры, читается при сборке контекста шага.
        private readonly ISimulationPipeline _globalPipeline; // Глобальный конвейер стадий.
        private readonly ContinuumService _continuumService; // Глобальный сервис межсистемных транзитов.

        private Thread _thread; // Выделенный поток исполнения глобальной симуляции.
        private bool _running; // Флаг жизненного цикла потока.
        private bool _disposed; // Защита от повторного Dispose.

        public SimulationGlobalWorker(GameStateService gameState, ISimulationPipeline globalPipeline, ContinuumService continuumService)
        {
            _gameState = gameState;
            _globalPipeline = globalPipeline;
            _continuumService = continuumService;
        }

        public void Start()
        {
            if (_running)
                return; // Уже запущено.

            _running = true;
            _thread = new Thread(ThreadLoop)
            {
                IsBackground = true, // Не блокируем завершение процесса.
                Name = "SimulationGlobalWorker" // Имя для диагностики/профайлера.
            };
            _thread.Start();
            GlobalSyncDebugLog.Log("GlobalWorker", "started");
        }

        public void EnqueueRunStep(int day, ERunMode mode, int activeSystemIndex)
        {
            if (!_running || _disposed)
                return; // В остановленном состоянии новые шаги не принимаем.

            int queued;
            lock (_queueLock)
            {
                _queue.Enqueue(new WorkItem
                {
                    Day = day, // Игровой день, который должен посчитать worker.
                    Mode = mode, // Режим симуляции на момент постановки шага.
                    ActiveSystemIndex = activeSystemIndex // Снимок active system на границе хода.
                });
                queued = _queue.Count;
            }

            _idle.Reset(); // Появилась работа — worker больше не idle.
            _signal.Set();
            GlobalSyncDebugLog.Log("GlobalWorker", "enqueue day=" + day + " mode=" + mode + " active=" + activeSystemIndex + " queue=" + queued);
        }

        public bool WaitForIdle(int timeoutMs)
        {
            if (_disposed)
                return true; // После Dispose считаем worker неактивным.

            if (timeoutMs <= 0)
                timeoutMs = 1; // Защита от некорректного таймаута.

            bool ok = _idle.Wait(timeoutMs);
            GlobalSyncDebugLog.Log("GlobalWorker", "wait-idle timeoutMs=" + timeoutMs + " result=" + ok);
            return ok;
        }

        public void Dispose()
        {
            if (_disposed)
                return; // Повторный вызов игнорируем.

            _disposed = true;
            _running = false; // Просим цикл ThreadLoop завершиться.
            _signal.Set(); // Будим поток, если он ждёт signal.

            var thread = _thread;
            if (thread != null && thread.IsAlive)
                thread.Join(1000); // Даем время на мягкую остановку.

            _signal.Dispose();
            _idle.Dispose();
            GlobalSyncDebugLog.Log("GlobalWorker", "disposed");
        }

        private void ThreadLoop()
        {
            while (_running)
            {
                WorkItem item;
                if (!TryDequeue(out item))
                {
                    _signal.WaitOne(50); // Периодическое ожидание новых задач.
                    continue;
                }

                GlobalSyncDebugLog.Log("GlobalWorker", "step-start day=" + item.Day + " mode=" + item.Mode + " active=" + item.ActiveSystemIndex);

                var bus = new SimulationEventBus();
                var context = new SimulationStepContext(
                    _gameState,
                    item.Day,
                    SimulationConsts.GlobalStepSeconds,
                    item.Mode,
                    bus,
                    item.ActiveSystemIndex,
                    true); // Контекст глобального шага всегда начинается на границе игрового хода.

                _continuumService?.Tick(in context);
                _globalPipeline?.RunStep(in context);
                GlobalSyncDebugLog.Log("GlobalWorker", "step-end day=" + item.Day);

                lock (_queueLock)
                {
                    if (_queue.Count == 0)
                        _idle.Set(); // Очередь пуста, worker вернулся в idle.
                }
            }
        }

        private bool TryDequeue(out WorkItem item)
        {
            lock (_queueLock)
            {
                if (_queue.Count == 0)
                {
                    item = default;
                    return false; // Нечего исполнять.
                }

                item = _queue.Dequeue(); // Берём следующую задачу FIFO.
                return true;
            }
        }

        private struct WorkItem
        {
            public int Day; // День симуляции для этого глобального шага.
            public ERunMode Mode; // Режим (Auto/Step/Paused) на момент постановки.
            public int ActiveSystemIndex; // Индекс активной системы, которую глобал должен пропустить.
        }
    }
}
