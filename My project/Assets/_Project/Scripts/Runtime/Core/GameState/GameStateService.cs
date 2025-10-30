using System;                                                     // Array.Empty
using _Project.Scripts.Galaxy.Data;                               // StarSys

namespace _Project.Scripts.Core.GameState
{
    public sealed class GameStateService
    {
        // ---- Снапшот (POD) ----
        public struct Snapshot
        {
            public ERunMode       RunMode;           // Paused | Step | Auto
            public EPlayStepSpeed PlayStepSpeed;     // X1 | X3 | X5
            public long           TickIndex;         // номер логического шага
            public float          LogicStepSeconds;  // Фикс. Длительность шага логики
            public bool           RequestStep;       // однокадровый флаг "выполнить один шаг"
            public StarSys[]      Galaxy;            // текущее состояние галактики
        }

        public struct RenderSnapshot
        {
            public ERunMode       RunMode;
            public EPlayStepSpeed PlayStepSpeed;
            public long           TickIndex;
            public float          LogicStepSeconds;
            public StarSys[]      Galaxy;
        }

        private Snapshot _current;
        private RenderSnapshot _render;

        public GameStateService(float logicStepSeconds)
        {
            _current = new Snapshot
            {
                RunMode          = ERunMode.Paused,
                PlayStepSpeed    = EPlayStepSpeed.X1,
                TickIndex        = 0,
                LogicStepSeconds = logicStepSeconds,
                RequestStep      = false,
                Galaxy           = Array.Empty<StarSys>()
            };

            _render = BuildRenderSnapshot(_current);
        }

        // ---- Чтение текущего состояния (без копий лишних объектов) ----
        public Snapshot Current => _current;

        public RenderSnapshot Render => _render;

        public StarSys[] GetGalaxy() => _current.Galaxy;

        // ---- Управление из UI ----
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
            Commit(snapshot);
        }

        public void AdvanceTick()
        {
            var snapshot = _current;
            snapshot.TickIndex++;
            Commit(snapshot);
        }

        // ---- Вспомогательное: длительность визуального проигрывания шага (сек) ----
        public void Commit(in Snapshot snapshot)
        {
            _current = snapshot;
            _render = BuildRenderSnapshot(_current);
        }

        private static RenderSnapshot BuildRenderSnapshot(in Snapshot snapshot)
        {
            return new RenderSnapshot
            {
                RunMode          = snapshot.RunMode,
                PlayStepSpeed    = snapshot.PlayStepSpeed,
                TickIndex        = snapshot.TickIndex,
                LogicStepSeconds = snapshot.LogicStepSeconds,
                Galaxy           = snapshot.Galaxy
            };
        }
    }
}
