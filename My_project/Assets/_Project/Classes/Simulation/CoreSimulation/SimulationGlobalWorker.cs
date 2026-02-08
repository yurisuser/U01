using System;
using System.Collections.Generic;
using System.Threading;
using _Project.Scripts.Const;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Simulation.Continuum;

namespace _Project.Scripts.Simulation.Core
{
    /// <summary>Отдельный поток глобальной симуляции: принимает команды хода и выполняет pipeline вне main thread.</summary>
    public sealed class SimulationGlobalWorker : IDisposable
    {
        private readonly object _queueLock = new object();
        private readonly Queue<WorkItem> _queue = new Queue<WorkItem>(8);
        private readonly AutoResetEvent _signal = new AutoResetEvent(false);

        private readonly GameStateService _gameState;
        private readonly ISimulationPipeline _globalPipeline;
        private readonly ContinuumService _continuumService;

        private Thread _thread;
        private bool _running;
        private bool _disposed;

        public SimulationGlobalWorker(GameStateService gameState, ISimulationPipeline globalPipeline, ContinuumService continuumService)
        {
            _gameState = gameState;
            _globalPipeline = globalPipeline;
            _continuumService = continuumService;
        }

        public void Start()
        {
            if (_running)
                return;

            _running = true;
            _thread = new Thread(ThreadLoop)
            {
                IsBackground = true,
                Name = "SimulationGlobalWorker"
            };
            _thread.Start();
        }

        public void EnqueueRunStep(int day, ERunMode mode, int activeSystemIndex)
        {
            if (!_running || _disposed)
                return;

            lock (_queueLock)
            {
                _queue.Enqueue(new WorkItem
                {
                    Day = day,
                    Mode = mode,
                    ActiveSystemIndex = activeSystemIndex
                });
            }

            _signal.Set();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _running = false;
            _signal.Set();

            var thread = _thread;
            if (thread != null && thread.IsAlive)
                thread.Join(1000);

            _signal.Dispose();
        }

        private void ThreadLoop()
        {
            while (_running)
            {
                WorkItem item;
                if (!TryDequeue(out item))
                {
                    _signal.WaitOne(50);
                    continue;
                }

                var bus = new SimulationEventBus();
                var context = new SimulationStepContext(
                    _gameState,
                    item.Day,
                    SimulationConsts.GlobalStepSeconds,
                    item.Mode,
                    bus,
                    item.ActiveSystemIndex);

                _continuumService?.Tick(in context);
                _globalPipeline?.RunStep(in context);
            }
        }

        private bool TryDequeue(out WorkItem item)
        {
            lock (_queueLock)
            {
                if (_queue.Count == 0)
                {
                    item = default;
                    return false;
                }

                item = _queue.Dequeue();
                return true;
            }
        }

        private struct WorkItem
        {
            public int Day;
            public ERunMode Mode;
            public int ActiveSystemIndex;
        }
    }
}
