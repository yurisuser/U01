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
                RequestStep      = false
            };
        }

        // ---- Чтение текущего состояния (без копий лишних объектов) ----
        public Snapshot Current => _current;

        // ---- Управление из UI ----
        public void SetRunMode(ERunMode mode)
        {
            _current.RunMode = mode;
        }

        public void SetPlayStepSpeed(EPlayStepSpeed speed)
        {
            _current.PlayStepSpeed = speed;
        }
        public void RequestStep()
        {
            _current.RequestStep = true;
        }
        public bool ConsumeStepRequest()
        {
            if (_current.RequestStep)
            {
                _current.RequestStep = false;
                return true;
            }
            return false;
        }

        public void AdvanceTick()
        {
            _current.TickIndex++;
        }

        // ---- Вспомогательное: длительность визуального проигрывания шага (сек) ----
        public float GetVisualStepDurationSeconds()
        {
            // Логика всегда тикает с LogicStepSeconds; визуал проигрывается быстрее/медленнее.
            return _current.LogicStepSeconds / (int)_current.PlayStepSpeed;
        }
    }
}
