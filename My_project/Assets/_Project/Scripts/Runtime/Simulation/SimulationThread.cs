using System;
using System.Threading;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Simulation.Execution;

namespace _Project.Scripts.Simulation
{
    // Запускает Executor в отдельном потоке, чтобы не блокировать основной цикл.
    public sealed class SimulationThread : IDisposable
    {
        private readonly Executor _executor; // Исполнитель шага симуляции.
        private readonly Thread _thread; // Поток-воркер.
        private readonly AutoResetEvent _wakeUp = new AutoResetEvent(false); // Событие для пробуждения потока.
        private readonly object _sync = new object(); // Лок для синхронизации.

        private bool _acceptTasks = true; // Можно ли ставить новые задачи.
        private bool _running     = true; // Поток ещё работает.
        private bool _hasTask; // Есть ли задача в очереди.
        private bool _hasResult; // Есть ли готовый результат.
        private bool _isProcessing; // Сейчас выполняется шаг.

        private GameStateService.Snapshot _taskSnapshot; // Снимок для работы.
        private GameStateService.Snapshot _resultSnapshot; // Результат последнего шага.
        private float _taskDt; // Дельта-время для задачи.

        // Создаём поток симуляции и сразу его запускаем.
        public SimulationThread(Executor executor)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));

            _thread = new Thread(ThreadLoop)
            {
                IsBackground = true,
                Name = "SimulationWorker"
            };
            _thread.Start();
        }

        // Ставим шаг симуляции в очередь, если поток готов.
        public bool TryScheduleStep(in GameStateService.Snapshot snapshot, float dt)
        {
            if (dt < 0f)
                dt = 0f;

            lock (_sync)
            {
                if (!_acceptTasks || _hasTask || _isProcessing)
                    return false;

                _taskSnapshot = snapshot;
                _taskDt       = dt;
                _hasTask      = true;
            }

            _wakeUp.Set();
            return true;
        }

        // Забираем готовый снимок шага, если он есть.
        public bool TryGetCompletedStep(out GameStateService.Snapshot snapshot)
        {
            lock (_sync)
            {
                if (!_hasResult)
                {
                    snapshot = default;
                    return false;
                }

                snapshot   = _resultSnapshot;
                _hasResult = false;
                return true;
            }
        }

        // Основной цикл фонового потока.
        private void ThreadLoop()
        {
            while (true)
            {
                _wakeUp.WaitOne();

                GameStateService.Snapshot snapshot;
                float dt;

                lock (_sync)
                {
                    if (!_running && !_hasTask)
                        break;

                    if (!_hasTask)
                        continue;

                    snapshot      = _taskSnapshot;
                    dt            = _taskDt;
                    _hasTask      = false;
                    _isProcessing = true;
                }

                _executor.Execute(ref snapshot, dt);

                lock (_sync)
                {
                    _resultSnapshot = snapshot;
                    _hasResult      = true;
                    _isProcessing   = false;

                    if (!_running && !_hasTask)
                        _wakeUp.Set();
                }
            }
        }

        // Останавливаем приём задач и дожидаемся завершения.
        public void Stop()
        {
            lock (_sync)
            {
                if (!_acceptTasks)
                    return;

                _acceptTasks = false;
                _running     = false;
            }

            _wakeUp.Set();
            _thread.Join();
        }

        // Освобождаем ресурсы воркера.
        public void Dispose()
        {
            Stop();
            _wakeUp.Dispose();
        }
    }
}
