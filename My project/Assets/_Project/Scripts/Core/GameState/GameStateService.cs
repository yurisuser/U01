namespace _Project.Scripts.Core.GameState
{
    /// <summary>
    /// Единственный источник истины для режима игры.
    /// Держит снапшот и даёт минимальные операции для UI и симуляции.
    /// Без ссылок на Unity сцены/объекты. Нулевые аллокации в рантайме.
    /// </summary>
    public sealed class GameStateService
    {
        // ---- Снапшот (POD) ----
        public struct Snapshot
        {
            public ERunMode       RunMode;           // Paused | Step | Auto
            public EPlayStepSpeed PlayStepSpeed;     // X1 | X3 | X5
            public long           TickIndex;         // номер логического шага
            public float          LogicStepSeconds;  // Фикс. Длительность шага логики (например, 1.0f)
            public bool           RequestStep;       // однокадровый флаг "выполнить один шаг"
        }

        private Snapshot _current;

        public GameStateService(float logicStepSeconds = 1.0f)
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

        /// <summary>
        /// Попросить выполнить ровно один логический шаг (используется в режиме Step/Paused).
        /// Флаг будет сброшен симуляцией через ConsumeStepRequest().
        /// </summary>
        public void RequestStep()
        {
            _current.RequestStep = true;
        }

        // ---- Вызывается ТОЛЬКО симуляцией ----

        /// <summary>
        /// Прочитать и сбросить одношаговый флаг. Возвращает true, если шаг запрошен.
        /// </summary>
        public bool ConsumeStepRequest()
        {
            if (_current.RequestStep)
            {
                _current.RequestStep = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Увеличить индекс логического шага после успешного DoStep().
        /// </summary>
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
