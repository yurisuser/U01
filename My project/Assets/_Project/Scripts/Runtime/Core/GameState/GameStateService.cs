using System;                                                     // Array.Empty
using _Project.Scripts.Core;                                      // UID
using _Project.Scripts.Galaxy.Data;                               // StarSys

namespace _Project.Scripts.Core.GameState
{
    public sealed class GameStateService
    {
        // ---- ������ (POD) ----
        public struct Snapshot
        {
            public ERunMode       RunMode;             // Paused | Step | Auto
            public EPlayStepSpeed PlayStepSpeed;       // X1 | X3 | X5
            public long           TickIndex;           // ����� �����᪮�� 蠣�
            public float          LogicStepSeconds;    // ����. ���⥫쭮��� 蠣� ������
            public bool           RequestStep;         // �������஢� 䫠� "�믮����� ���� 蠣"
            public StarSys[]      Galaxy;              // ⥪�饥 ���ﭨ� �����⨪�
            public int            SelectedSystemIndex; // -1, ���� �� ���⥫��
        }

        public struct RenderSnapshot
        {
            public ERunMode       RunMode;
            public EPlayStepSpeed PlayStepSpeed;
            public long           TickIndex;
            public float          LogicStepSeconds;
            public StarSys[]      Galaxy;
            public int            SelectedSystemIndex;
        }

        private Snapshot _current;
        private RenderSnapshot _render;

        public event Action<Snapshot> SnapshotChanged;          // ��������� ��� �������� �����
        public event Action<RenderSnapshot> RenderChanged;      // ��������� ��� UI/рендеров

        public GameStateService(float logicStepSeconds)
        {
            _current = new Snapshot
            {
                RunMode              = ERunMode.Paused,
                PlayStepSpeed        = EPlayStepSpeed.X1,
                TickIndex            = 0,
                LogicStepSeconds     = logicStepSeconds,
                RequestStep          = false,
                Galaxy               = Array.Empty<StarSys>(),
                SelectedSystemIndex  = -1
            };

            _render = BuildRenderSnapshot(_current);
        }

        // ---- �⥭�� ⥪�饣� ���ﭨ� (��� ����� ��譨� ��ꥪ⮢) ----
        public Snapshot Current => _current;

        public RenderSnapshot Render => _render;

        public StarSys[] GetGalaxy() => _current.Galaxy;

        public StarSys? GetSelectedSystem()
        {
            return TryGetSystem(_current.SelectedSystemIndex, _current.Galaxy);
        }

        // ---- ��ࠢ����� �� UI ----
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
            if (seconds <= 0f) return;
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
            if (!_current.RequestStep) return;
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
            if (galaxy == null || galaxy.Length == 0) return false;

            for (int i = 0; i < galaxy.Length; i++)
            {
                var sys = galaxy[i];
                if (sys.Uid.Type == uid.Type && sys.Uid.Id == uid.Id)
                {
                    return SelectSystemByIndex(i);
                }
            }

            return false;
        }

        public void ClearSelectedSystem()
        {
            if (_current.SelectedSystemIndex == -1) return;
            var snapshot = _current;
            snapshot.SelectedSystemIndex = -1;
            Commit(snapshot);
        }

        // ---- �ᯮ����⥫쭮�: ���⥫쭮��� ���㠫쭮�� �ந��뢠��� 蠣� (ᥪ) ----
        public void Commit(in Snapshot snapshot)
        {
            var previousRender = _render;

            _current = snapshot;
            _render  = BuildRenderSnapshot(_current);

            SnapshotChanged?.Invoke(_current);

            bool renderDirty =
                previousRender.RunMode             != _render.RunMode ||
                previousRender.PlayStepSpeed       != _render.PlayStepSpeed ||
                previousRender.LogicStepSeconds    != _render.LogicStepSeconds ||
                previousRender.SelectedSystemIndex != _render.SelectedSystemIndex ||
                !ReferenceEquals(previousRender.Galaxy, _render.Galaxy);

            if (renderDirty)
                RenderChanged?.Invoke(_render);
        }

        private static RenderSnapshot BuildRenderSnapshot(in Snapshot snapshot)
        {
            return new RenderSnapshot
            {
                RunMode              = snapshot.RunMode,
                PlayStepSpeed        = snapshot.PlayStepSpeed,
                TickIndex            = snapshot.TickIndex,
                LogicStepSeconds     = snapshot.LogicStepSeconds,
                Galaxy               = snapshot.Galaxy,
                SelectedSystemIndex  = snapshot.SelectedSystemIndex
            };
        }

        private static StarSys? TryGetSystem(int index, StarSys[] galaxy)
        {
            if (galaxy == null || galaxy.Length == 0) return null;
            if (index < 0 || index >= galaxy.Length) return null;
            return galaxy[index];
        }
    }
}
