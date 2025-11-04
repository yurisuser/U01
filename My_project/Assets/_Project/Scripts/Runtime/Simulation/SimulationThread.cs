using System;
using System.Threading;
using _Project.Scripts.Core.GameState;

namespace _Project.Scripts.Simulation
{
    /// <summary>
    /// Runs Executor on a dedicated background thread so simulation work no longer blocks the main loop.
    /// </summary>
    public sealed class SimulationThread : IDisposable
    {
        private readonly Executor _executor;
        private readonly Thread _thread;
        private readonly AutoResetEvent _wakeUp = new AutoResetEvent(false);
        private readonly object _sync = new object();

        private bool _acceptTasks = true;
        private bool _running     = true;
        private bool _hasTask;
        private bool _hasResult;
        private bool _isProcessing;

        private GameStateService.Snapshot _taskSnapshot;
        private GameStateService.Snapshot _resultSnapshot;
        private float _taskDt;

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

        public void Dispose()
        {
            Stop();
            _wakeUp.Dispose();
        }
    }
}
