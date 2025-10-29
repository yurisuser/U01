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

        private Snapshot _current;

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
        }

        // ---- Чтение текущего состояния (без копий лишних объектов) ----
        public Snapshot Current => _current;

        public StarSys[] GetGalaxy() => _current.Galaxy;

        // ---- Управление из UI ----
        public void SetRunMode(ERunMode mode)
        {
            _current.RunMode = mode;
        }

        public void SetGalaxy(StarSys[] galaxy)
        {
            _current.Galaxy = galaxy ?? Array.Empty<StarSys>();
        }

        public void AdvanceTick()
        {
            _current.TickIndex++;
        }

        // ---- Вспомогательное: длительность визуального проигрывания шага (сек) ----
    }
}
