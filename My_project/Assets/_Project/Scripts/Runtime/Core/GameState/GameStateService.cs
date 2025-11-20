using System;
using _Project.Scripts.Core;
using _Project.Scripts.Core.Runtime;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.Render;
using System.Collections.Generic;

namespace _Project.Scripts.Core.GameState
{
    // Хранит логическое состояние игры и снимки для визуализации.
    public sealed class GameStateService
    {
        // ----- данные для логики -----
        public struct Snapshot
        {
            public ERunMode       RunMode; // Текущий режим (пауза/игра).
            public EPlayStepSpeed PlayStepSpeed; // Скорость воспроизведения.
            public long           TickIndex; // Номер шага.
            public float          LogicStepSeconds; // Длительность шага.
            public bool           RequestStep; // Запрос одиночного шага.
            public StarSys[]      Galaxy; // Копия данных галактики.
            public int            SelectedSystemIndex; // Выбранная система.
        }

        // ----- данные для UI -----
        public struct RenderSnapshot
        {
            public ERunMode       RunMode; // Состояние симуляции.
            public EPlayStepSpeed PlayStepSpeed; // Выбранная скорость.
            public long           TickIndex; // Текущий тик.
            public float          LogicStepSeconds; // Длительность шага.
            public StarSys[]      Galaxy; // Ссылка на исходные данные галактики.
            public int            SelectedSystemIndex; // Активная система.
            public Ship[]         PreviousShips; // Корабли на прошлый шаг.
            public int            PreviousShipCount; // Сколько кораблей в previous.
            public Ship[]         CurrentShips; // Корабли текущего снапшота.
            public int            CurrentShipCount; // Их количество.
            public Ship[]         NextShips; // Буфер следующего шага.
            public int            NextShipCount; // Количество в next.
            public int            ShipsVersion; // Версия, чтобы UI понимал обновления.
            public float          StepProgress; // Прогресс между снапшотами.
            public IReadOnlyDictionary<UID, List<SubstepSample>> Substeps; // Трейсы сабстепов.
            public int            SubstepsVersion; // Версия сабстепов.
            public int            SubstepsSystemIndex; // Для какой системы трейсы.
        }

        private Snapshot _current; // Текущее логическое состояние.
        private RenderSnapshot _render; // Последний снимок для UI.
        private RuntimeContext _runtimeContext; // Контекст симуляции.

        private Ship[] _shipsPrev = Array.Empty<Ship>(); // Буфер кораблей T-1.
        private Ship[] _shipsCurr = Array.Empty<Ship>(); // Буфер кораблей T0.
        private Ship[] _shipsNext = Array.Empty<Ship>(); // Буфер кораблей T1.
        private int _shipsPrevCount;
        private int _shipsCurrCount;
        private int _shipsNextCount;
        private int _shipsVersion; // Индикатор обновлений кораблей.
        private int _lastDynamicSystemIndex = -1; // Последняя система для снапшота.
        private float _stepProgress; // Прогресс интерполяции.
        private volatile bool _dynamicDirty; // Требуется ли пересборка динамики.
        private bool _forceRebuildCurrentShips; // Форсируем rebuild текущего буфера.
        private IReadOnlyDictionary<UID, List<SubstepSample>> _substeps; // Последние сабстепы.
        private int _substepsVersion; // Версия сабстепов.
        private int _substepsSystemIndex = -1; // Для какой системы они актуальны.

        public event Action<Snapshot> SnapshotChanged; // UI/логика подписываются на изменения логики.
        public event Action<RenderSnapshot> RenderChanged; // UI подписывается на визуальные изменения.

        // Инициализируем сервис и создаём начальные снапшоты.
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

        public Snapshot Current => _current; // Текущий снимок для логики.
        public RenderSnapshot Render => _render; // Последний снимок для UI.

        public StarSys[] GetGalaxy() => _current.Galaxy; // Получить текущую галактику.

        public StarSys? GetSelectedSystem()
        {
            return TryGetSystem(_current.SelectedSystemIndex, _current.Galaxy);
        }

        // Подключаем контекст симуляции, чтобы читать данные о кораблях.
        public void AttachRuntimeContext(RuntimeContext context)
        {
            _runtimeContext = context;
            MarkDynamicDirty();
            _forceRebuildCurrentShips = true;
            CheckDynamicSnapshot(_current.SelectedSystemIndex);
            _render = BuildRenderSnapshot(_current);
            RenderChanged?.Invoke(_render);
        }

        // ----- управление из UI -----

        public void SetRunMode(ERunMode mode) // Меняем режим (пауза/игра).
        {
            var snapshot = _current;
            snapshot.RunMode = mode;
            Commit(snapshot);
        }

        public void SetGalaxy(StarSys[] galaxy) // Загружаем галактику и корректируем выбор.
        {
            var snapshot = _current;
            snapshot.Galaxy = galaxy ?? Array.Empty<StarSys>();

            if (snapshot.Galaxy.Length == 0)
                snapshot.SelectedSystemIndex = -1;
            else if (snapshot.SelectedSystemIndex < 0 || snapshot.SelectedSystemIndex >= snapshot.Galaxy.Length)
                snapshot.SelectedSystemIndex = 0;

            Commit(snapshot);
        }

        public void SetPlayStepSpeed(EPlayStepSpeed speed) // Меняем скорость воспроизведения.
        {
            var snapshot = _current;
            snapshot.PlayStepSpeed = speed;
            Commit(snapshot);
        }

        public void SetLogicStepSeconds(float seconds) // Устанавливаем длительность шага.
        {
            if (seconds <= 0f)
                return;

            var snapshot = _current;
            snapshot.LogicStepSeconds = seconds;
            Commit(snapshot);
        }

        public void RequestStep() // Запрос одиночного шага (для пошагового режима).
        {
            var snapshot = _current;
            snapshot.RequestStep = true;
            Commit(snapshot);
        }

        public void ClearStepRequest() // Сбрасываем запрос шага.
        {
            if (!_current.RequestStep)
                return;

            var snapshot = _current;
            snapshot.RequestStep = false;
            Commit(snapshot);
        }

        public void AdvanceTick() // Инкрементируем счётчик тиков.
        {
            var snapshot = _current;
            snapshot.TickIndex++;
            Commit(snapshot);
        }

        // Обновляем сабстепы для активной системы.
        public void SetSubstepTraces(IReadOnlyDictionary<UID, List<SubstepSample>> traces, int systemIndex)
        {
            _substeps = traces;
            _substepsSystemIndex = systemIndex;
            _substepsVersion++;
            UpdateRenderSnapshot();
        }

        // Выбираем систему по индексу и обновляем снапшоты.
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

        // Выбираем систему по UID.
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

        // Снимаем выбор системы.
        public void ClearSelectedSystem()
        {
            if (_current.SelectedSystemIndex == -1)
                return;

            var snapshot = _current;
            snapshot.SelectedSystemIndex = -1;
            Commit(snapshot);
        }

        // ----- обновление состояния -----

        // Применяем обновлённый снапшот и уведомляем слушателей.
        public void Commit(in Snapshot snapshot)
        {
            _current = snapshot;
            SnapshotChanged?.Invoke(_current);
            UpdateRenderSnapshot();
        }

        /// <summary>
        /// Отмечаем, что динамический снимок (корабли) устарел и его надо перечитать.
        /// Вызывает Executor после расчёта шага.
        /// </summary>
        internal void MarkDynamicDirty() // Помечаем, что динамический буфер устарел.
        {
            _dynamicDirty = true;
        }

        // Продвигаем буферы T-1 <- T0 <- T+1, если есть данные нового шага.
        internal bool TryPromoteNextShips()
        {
            if (_shipsNextCount <= 0)
                return false;

            var oldPrev       = _shipsPrev;

            _shipsPrev       = _shipsCurr;
            _shipsPrevCount  = _shipsCurrCount;
            _shipsCurr       = _shipsNext;
            _shipsCurrCount  = _shipsNextCount;
            _shipsNext       = oldPrev;
            _shipsNextCount  = 0;
            _shipsVersion++;
            _stepProgress = 0f;

            UpdateRenderSnapshot();
            return true;
        }

        /// <summary>
        /// Обновляет прогресс логического шага (0..1) и уведомляет UI при изменении.
        /// </summary>
        internal void SetStepProgress(float progress) // Обновляем прогресс шага.
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
        public void RefreshDynamicSnapshot() // Полностью перечитываем текущие корабли.
        {
            MarkDynamicDirty();
            _forceRebuildCurrentShips = true;
            CheckDynamicSnapshot(_current.SelectedSystemIndex);
            UpdateRenderSnapshot();
        }

        // Следим за буферами кораблей и перечитываем их при необходимости.
        private void CheckDynamicSnapshot(int systemIndex)
        {
            if (_runtimeContext?.Systems == null)
            {
                _shipsPrev      = Array.Empty<Ship>();
                _shipsCurr      = Array.Empty<Ship>();
                _shipsNext      = Array.Empty<Ship>();
                _shipsPrevCount = 0;
                _shipsCurrCount = 0;
                _shipsNextCount = 0;
                _lastDynamicSystemIndex = -1;
                _dynamicDirty = false;
                _forceRebuildCurrentShips = false;
                _stepProgress = 0f;
                return;
            }

            bool systemChanged = systemIndex != _lastDynamicSystemIndex || _forceRebuildCurrentShips;
            if (!_dynamicDirty && !systemChanged)
                return;

            if (systemIndex < 0 || systemIndex >= _runtimeContext.Systems.Count)
            {
                _shipsPrevCount = 0;
                _shipsCurrCount = 0;
                _shipsNextCount = 0;
                _lastDynamicSystemIndex = systemIndex;
                _dynamicDirty = false;
                _forceRebuildCurrentShips = false;
                return;
            }

            if (systemChanged)
            {
                _shipsCurrCount = _runtimeContext.Systems.CopyShipsToBuffer(systemIndex, ref _shipsCurr);
                EnsureBufferCapacity(ref _shipsPrev, _shipsCurrCount);
                if (_shipsCurrCount > 0)
                    Array.Copy(_shipsCurr, 0, _shipsPrev, 0, _shipsCurrCount);
                _shipsPrevCount = _shipsCurrCount;

                EnsureBufferCapacity(ref _shipsNext, _shipsCurrCount);
                _shipsNextCount = 0;

                _shipsVersion++;
                _stepProgress = 0f;
                _lastDynamicSystemIndex = systemIndex;
                _dynamicDirty = false;
                _forceRebuildCurrentShips = false;
                return;
            }

            _shipsNextCount = _runtimeContext.Systems.CopyShipsToBuffer(systemIndex, ref _shipsNext);
            _dynamicDirty = false;
        }

        // Собираем RenderSnapshot для UI на основе текущего состояния.
        private RenderSnapshot BuildRenderSnapshot(in Snapshot snapshot)
        {
            CheckDynamicSnapshot(snapshot.SelectedSystemIndex);

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
                NextShips           = _shipsNext,
                NextShipCount       = _shipsNextCount,
                ShipsVersion        = _shipsVersion,
                StepProgress        = _stepProgress,
                Substeps            = snapshot.SelectedSystemIndex == _substepsSystemIndex ? _substeps : null,
                SubstepsVersion     = _substepsVersion,
                SubstepsSystemIndex = _substepsSystemIndex
            };
        }

        // Проверяем, изменилось ли что-то для UI, чтобы не слать лишние события.
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
                previous.SubstepsVersion     != next.SubstepsVersion ||
                !ReferenceEquals(previous.Galaxy, next.Galaxy) ||
                !ReferenceEquals(previous.CurrentShips, next.CurrentShips) ||
                !ReferenceEquals(previous.PreviousShips, next.PreviousShips) ||
                !ReferenceEquals(previous.NextShips, next.NextShips);
        }

        // Строим новый RenderSnapshot и уведомляем UI при изменении.
        private void UpdateRenderSnapshot()
        {
            var previousRender = _render;
            _render = BuildRenderSnapshot(_current);

            if (IsRenderDirty(previousRender, _render))
                RenderChanged?.Invoke(_render);
        }

        // Гарантируем, что буфер кораблей вмещает запрошенное количество.
        private static void EnsureBufferCapacity(ref Ship[] buffer, int needed)
        {
            if (needed <= 0)
            {
                if (buffer == null)
                    buffer = Array.Empty<Ship>();
                return;
            }

            if (buffer == null || buffer.Length < needed)
                Array.Resize(ref buffer, needed);
        }

        // Безопасно возвращаем систему по индексу.
        private static StarSys? TryGetSystem(int index, StarSys[] galaxy)
        {
            if (galaxy == null || galaxy.Length == 0)
                return null;

            if (index < 0 || index >= galaxy.Length)
                return null;

            return galaxy[index];
        }

        // Клэмп [0;1] без использования Mathf (для работы вне Unity).
        private static float Clamp01(float value)
        {
            if (value < 0f) return 0f;
            if (value > 1f) return 1f;
            return value;
        }
    }
}
