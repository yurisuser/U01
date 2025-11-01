using System;
using _Project.Scripts.Core;
using _Project.Scripts.Core.Runtime;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;

namespace _Project.Scripts.Core.GameState
{
    /// <summary>
    /// Хранит текущее состояние игры и снапшот для UI.
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
            public Ship[]         Ships;
            public int            ShipCount;
            public int            ShipsVersion; // увеличивается при каждом обновлении динамики
        }

        private Snapshot _current;
        private RenderSnapshot _render;
        private RuntimeContext _runtimeContext;
        private Ship[] _renderShipsBuffer = Array.Empty<Ship>();
        private int _shipsVersion;

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
            if (_runtimeContext != null)
                RefreshDynamicSnapshot();
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
            var previousRender = _render;
            _render = BuildRenderSnapshot(_current);

            SnapshotChanged?.Invoke(_current);

            if (IsRenderDirty(previousRender, _render))
                RenderChanged?.Invoke(_render);
        }

        /// <summary>
        /// Обновляем только динамические данные (корабли) без изменения Snapshot.
        /// Вызывать после того, как мир изменился, но состояние логики осталось прежним.
        /// </summary>
        public void RefreshDynamicSnapshot()
        {
            if (_runtimeContext == null)
                return;

            var previousRender = _render;
            var updatedRender = BuildRenderSnapshot(_current);

            if (!IsRenderDirty(previousRender, updatedRender))
                return;

            _render = updatedRender;
            RenderChanged?.Invoke(_render);
        }

        private RenderSnapshot BuildRenderSnapshot(in Snapshot snapshot)
        {
            var render = new RenderSnapshot
            {
                RunMode             = snapshot.RunMode,
                PlayStepSpeed       = snapshot.PlayStepSpeed,
                TickIndex           = snapshot.TickIndex,
                LogicStepSeconds    = snapshot.LogicStepSeconds,
                Galaxy              = snapshot.Galaxy,
                SelectedSystemIndex = snapshot.SelectedSystemIndex,
                Ships               = Array.Empty<Ship>(),
                ShipCount           = 0,
                ShipsVersion        = _shipsVersion
            };

            if (_runtimeContext != null &&
                _runtimeContext.Systems != null &&
                snapshot.Galaxy != null &&
                snapshot.Galaxy.Length > 0)
            {
                var systemIndex = snapshot.SelectedSystemIndex;
                if (systemIndex >= 0 && systemIndex < _runtimeContext.Systems.Count)
                {
                    var count = _runtimeContext.Systems.CopyShipsToBuffer(systemIndex, ref _renderShipsBuffer);
                    render.ShipCount = count;
                    render.Ships = count > 0 ? _renderShipsBuffer : Array.Empty<Ship>();
                    _shipsVersion++;
                    render.ShipsVersion = _shipsVersion;
                }
            }

            return render;
        }

        private static bool IsRenderDirty(in RenderSnapshot previous, in RenderSnapshot next)
        {
            return
                previous.RunMode             != next.RunMode ||
                previous.PlayStepSpeed       != next.PlayStepSpeed ||
                previous.LogicStepSeconds    != next.LogicStepSeconds ||
                previous.SelectedSystemIndex != next.SelectedSystemIndex ||
                previous.TickIndex           != next.TickIndex ||
                previous.ShipCount           != next.ShipCount ||
                previous.ShipsVersion        != next.ShipsVersion ||
                !ReferenceEquals(previous.Galaxy, next.Galaxy);
        }

        private static StarSys? TryGetSystem(int index, StarSys[] galaxy)
        {
            if (galaxy == null || galaxy.Length == 0)
                return null;

            if (index < 0 || index >= galaxy.Length)
                return null;

            return galaxy[index];
        }
    }
}
