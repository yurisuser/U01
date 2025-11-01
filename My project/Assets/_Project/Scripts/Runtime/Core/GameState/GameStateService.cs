using System;
using _Project.Scripts.Core;
using _Project.Scripts.Core.Runtime;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;

namespace _Project.Scripts.Core.GameState
{
    /// <summary>
    /// Хранит логическое состояние игры и снапшоты для визуализации.
    /// </summary>
    public sealed class GameStateService
    {
        // ----- данные для логики -----
        public struct Snapshot
        {
            public ERunMode       RunMode;
            public EPlayStepSpeed PlayStepSpeed;
            public long           TickIndex;
            public float          LogicStepSeconds;
            public bool           RequestStep;
            public StarSys[]      Galaxy;
            public int            SelectedSystemIndex;
        }

        // ----- данные для UI -----
        public struct RenderSnapshot
        {
            public ERunMode       RunMode;
            public EPlayStepSpeed PlayStepSpeed;
            public long           TickIndex;
            public float          LogicStepSeconds;
            public StarSys[]      Galaxy;
            public int            SelectedSystemIndex;
            public Ship[]         PreviousShips;
            public int            PreviousShipCount;
            public Ship[]         CurrentShips;
            public int            CurrentShipCount;
            public int            ShipsVersion;
            public float          StepProgress;
        }

        private Snapshot _current;
        private RenderSnapshot _render;
        private RuntimeContext _runtimeContext;

        private Ship[] _shipsPrev = Array.Empty<Ship>();
        private Ship[] _shipsCurr = Array.Empty<Ship>();
        private int _shipsPrevCount;
        private int _shipsCurrCount;
        private int _shipsVersion;
        private int _lastDynamicSystemIndex = -1;
        private float _stepProgress;
        private bool _dynamicDirty;

        public event Action<Snapshot> SnapshotChanged;
        public event Action<RenderSnapshot> RenderChanged;

        public GameStateService(float logicStepSeconds)
        {
            _current = new Snapshot
            {
                RunMode             = ERunMode.Paused,
                PlayStepSpeed       = EPlayStepSpeed.X1,
                TickIndex           = 0,
                LogicStepSeconds    = logicStepSeconds,
                RequestStep         = false,
                Galaxy              = Array.Empty<StarSys>(),
                SelectedSystemIndex = -1
            };

            _render = BuildRenderSnapshot(_current);
        }

        public Snapshot Current => _current;
        public RenderSnapshot Render => _render;

        public StarSys[] GetGalaxy() => _current.Galaxy;

        public StarSys? GetSelectedSystem()
        {
            return TryGetSystem(_current.SelectedSystemIndex, _current.Galaxy);
        }

        public void AttachRuntimeContext(RuntimeContext context)
        {
            _runtimeContext = context;
            MarkDynamicDirty();
            EnsureDynamicSnapshot(_current.SelectedSystemIndex);
            _render = BuildRenderSnapshot(_current);
            RenderChanged?.Invoke(_render);
        }

        // ----- управление из UI -----

        public void SetRunMode(ERunMode mode)
        {
            var snapshot = _current;
            snapshot.RunMode = mode;
            Commit(snapshot);
        }

        public void SetGalaxy(StarSys[] galaxy)
        {
            var snapshot = _current;
            snapshot.Galaxy = galaxy ?? Array.Empty<StarSys>();

            if (snapshot.Galaxy.Length == 0)
                snapshot.SelectedSystemIndex = -1;
            else if (snapshot.SelectedSystemIndex < 0 || snapshot.SelectedSystemIndex >= snapshot.Galaxy.Length)
                snapshot.SelectedSystemIndex = 0;

            Commit(snapshot);
        }

        public void SetPlayStepSpeed(EPlayStepSpeed speed)
        {
            var snapshot = _current;
            snapshot.PlayStepSpeed = speed;
            Commit(snapshot);
        }

        public void SetLogicStepSeconds(float seconds)
        {
            if (seconds <= 0f)
                return;

            var snapshot = _current;
            snapshot.LogicStepSeconds = seconds;
            Commit(snapshot);
        }

        public void RequestStep()
        {
            var snapshot = _current;
            snapshot.RequestStep = true;
            Commit(snapshot);
        }

        public void ClearStepRequest()
        {
            if (!_current.RequestStep)
                return;

            var snapshot = _current;
            snapshot.RequestStep = false;
            Commit(snapshot);
        }

        public void AdvanceTick()
        {
            var snapshot = _current;
            snapshot.TickIndex++;
            Commit(snapshot);
        }

        public bool SelectSystemByIndex(int index)
        {
            var snapshot = _current;
            if (snapshot.Galaxy == null || snapshot.Galaxy.Length == 0)
            {
                snapshot.SelectedSystemIndex = -1;
                Commit(snapshot);
                return false;
            }

            if (index < 0)
                index = 0;
            else if (index >= snapshot.Galaxy.Length)
                index = snapshot.Galaxy.Length - 1;

            if (snapshot.SelectedSystemIndex == index)
                return true;

            snapshot.SelectedSystemIndex = index;
            Commit(snapshot);
            return true;
        }

        public bool SelectSystemByUid(UID uid)
        {
            var galaxy = _current.Galaxy;
            if (galaxy == null || galaxy.Length == 0)
                return false;

            for (int i = 0; i < galaxy.Length; i++)
            {
                var sys = galaxy[i];
                if (sys.Uid.Type == uid.Type && sys.Uid.Id == uid.Id)
                    return SelectSystemByIndex(i);
            }

            return false;
        }

        public void ClearSelectedSystem()
        {
            if (_current.SelectedSystemIndex == -1)
                return;

            var snapshot = _current;
            snapshot.SelectedSystemIndex = -1;
            Commit(snapshot);
        }

        // ----- обновление состояния -----

        public void Commit(in Snapshot snapshot)
        {
            _current = snapshot;
            EnsureDynamicSnapshot(snapshot.SelectedSystemIndex);

            var previousRender = _render;
            _render = BuildRenderSnapshot(_current);

            SnapshotChanged?.Invoke(_current);

            if (IsRenderDirty(previousRender, _render))
                RenderChanged?.Invoke(_render);
        }

        /// <summary>
        /// Отмечаем, что динамический снимок (корабли) устарел и его надо перечитать.
        /// Вызывает Executor после расчёта шага.
        /// </summary>
        internal void MarkDynamicDirty()
        {
            _dynamicDirty = true;
        }

        /// <summary>
        /// Обновляет прогресс логического шага (0..1) и уведомляет UI при изменении.
        /// </summary>
        internal void SetStepProgress(float progress)
        {
            progress = Clamp01(progress);
            if (Math.Abs(_stepProgress - progress) < 0.0001f)
                return;

            _stepProgress = progress;
            _render.StepProgress = _stepProgress;
            RenderChanged?.Invoke(_render);
        }

        /// <summary>
        /// Полностью перечитать динамический снимок (например, при смене выбора в UI).
        /// </summary>
        public void RefreshDynamicSnapshot()
        {
            MarkDynamicDirty();
            EnsureDynamicSnapshot(_current.SelectedSystemIndex);

            var previousRender = _render;
            _render = BuildRenderSnapshot(_current);

            if (IsRenderDirty(previousRender, _render))
                RenderChanged?.Invoke(_render);
        }

        private void EnsureDynamicSnapshot(int systemIndex)
        {
            if (_runtimeContext?.Systems == null)
            {
                _shipsPrevCount = 0;
                _shipsCurrCount = 0;
                _lastDynamicSystemIndex = -1;
                _dynamicDirty = false;
                return;
            }

            bool systemChanged = systemIndex != _lastDynamicSystemIndex;
            if (!_dynamicDirty && !systemChanged)
                return;

            if (systemIndex < 0 || systemIndex >= _runtimeContext.Systems.Count)
            {
                _shipsPrevCount = 0;
                _shipsCurrCount = 0;
                _lastDynamicSystemIndex = systemIndex;
                _dynamicDirty = false;
                return;
            }

            SwapShipBuffers();
            _shipsCurrCount = _runtimeContext.Systems.CopyShipsToBuffer(systemIndex, ref _shipsCurr);
            _shipsVersion++;
            _lastDynamicSystemIndex = systemIndex;
            _dynamicDirty = false;
            _stepProgress = 0f;
        }

        private void SwapShipBuffers()
        {
            var temp = _shipsPrev;
            _shipsPrev = _shipsCurr;
            _shipsCurr = temp;

            _shipsPrevCount = _shipsCurrCount;
        }

        private RenderSnapshot BuildRenderSnapshot(in Snapshot snapshot)
        {
            EnsureDynamicSnapshot(snapshot.SelectedSystemIndex);

            return new RenderSnapshot
            {
                RunMode             = snapshot.RunMode,
                PlayStepSpeed       = snapshot.PlayStepSpeed,
                TickIndex           = snapshot.TickIndex,
                LogicStepSeconds    = snapshot.LogicStepSeconds,
                Galaxy              = snapshot.Galaxy,
                SelectedSystemIndex = snapshot.SelectedSystemIndex,
                PreviousShips       = _shipsPrev,
                PreviousShipCount   = _shipsPrevCount,
                CurrentShips        = _shipsCurr,
                CurrentShipCount    = _shipsCurrCount,
                ShipsVersion        = _shipsVersion,
                StepProgress        = _stepProgress
            };
        }

        private static bool IsRenderDirty(in RenderSnapshot previous, in RenderSnapshot next)
        {
            return
                previous.RunMode             != next.RunMode ||
                previous.PlayStepSpeed       != next.PlayStepSpeed ||
                previous.LogicStepSeconds    != next.LogicStepSeconds ||
                previous.SelectedSystemIndex != next.SelectedSystemIndex ||
                previous.TickIndex           != next.TickIndex ||
                previous.ShipsVersion        != next.ShipsVersion ||
                previous.StepProgress        != next.StepProgress ||
                !ReferenceEquals(previous.Galaxy, next.Galaxy) ||
                !ReferenceEquals(previous.CurrentShips, next.CurrentShips) ||
                !ReferenceEquals(previous.PreviousShips, next.PreviousShips);
        }

        private static StarSys? TryGetSystem(int index, StarSys[] galaxy)
        {
            if (galaxy == null || galaxy.Length == 0)
                return null;

            if (index < 0 || index >= galaxy.Length)
                return null;

            return galaxy[index];
        }

        private static float Clamp01(float value)
        {
            if (value < 0f) return 0f;
            if (value > 1f) return 1f;
            return value;
        }
    }
}
