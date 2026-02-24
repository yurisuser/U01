using System;
using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Galaxy.Generation;
using _Project.Scripts.Simulation.Continuum;
using _Project.Scripts.Simulation.Global.Debug;
using _Project.Scripts.Core.GameState.GameStateMembers;

namespace _Project.Scripts.Core.GameState
{
    /// <summary>Минимальное состояние игры: режим, галактика и выбранная система.</summary>
    public sealed class GameStateService
    {
        private StarSys[] _galaxy = Array.Empty<StarSys>(); // Массив всех систем галактики (основной mutable state).
        private HyperlinkEdge[] _hyperlinkEdges = Array.Empty<HyperlinkEdge>(); // Кэш графа гиперсвязей между системами.
        private int[][] _constellationList = Array.Empty<int[]>(); // Группировка систем по созвездиям.
        private ERunMode _runMode = ERunMode.Paused; // Текущий режим симуляции (Paused/Step/Auto).
        private readonly SelectedService _selectedService = new SelectedService(); // Сервис текущего выбора в UI.
        private int _activeLocalSystemIndex = -1; // Система, которая реально тикается локальным пайплайном (SystemMap).
        private bool _showHyperlinks = true; // Флаг показа гиперлинков в представлении.
        private bool _useHyperlinkColoring = true; // Флаг раскраски по гиперлинкам.
        private bool _useFractionColoring = true; // Флаг раскраски по фракциям.
        private bool _useSecurityColoring; // Флаг раскраски по уровню безопасности.
        private int _currentTurnNumber; // Текущий номер игрового хода (день глобальной симуляции).
        private Func<bool> _waitGlobalIdle; // Колбэк ожидания idle у global worker (handshake выбора системы).
        private ERunMode _runModeBeforeSelection = ERunMode.Paused; // Режим до входа в handshake выбора системы.
        private bool _selectionHandshakeActive; // Защита от повторного входа в handshake выбора системы.

        public event Action StateChanged; // Уведомление для UI/логики о смене состояния.

        public GameStateService()
        {
            _showHyperlinks = SettingsService.Instance.ShowHyperlinks;
            _useHyperlinkColoring = SettingsService.Instance.UseHyperlinkColoring;
            _useFractionColoring = SettingsService.Instance.UseFractionColoring;
            _useSecurityColoring = SettingsService.Instance.UseSecurityColoring;
        }

        public ERunMode RunMode => _runMode; // Публичный read-only доступ к текущему run mode.
        public StarSys[] Galaxy => _galaxy; // Публичный доступ к массиву систем.
        public HyperlinkEdge[] HyperlinkEdges => _hyperlinkEdges; // Публичный доступ к графу гиперсвязей.
        public int[][] ConstellationList => _constellationList; // Публичный доступ к списку созвездий.
        public int SelectedSystemIndex => _selectedService.SelectedSystemService.SelectedSystemIndex; // Индекс системы, выбранной в UI (галактическая карта).
        public int ActiveLocalSystemIndex => _activeLocalSystemIndex; // Индекс системы, активной для локальной симуляции.
        public bool ShowHyperlinks => _showHyperlinks; // UI-флаг показа гиперлинков.
        public bool UseHyperlinkColoring => _useHyperlinkColoring; // UI-флаг раскраски по гиперлинкам.
        public bool UseFractionColoring => _useFractionColoring; // UI-флаг раскраски по фракциям.
        public bool UseSecurityColoring => _useSecurityColoring; // UI-флаг раскраски по security level.
        public int CurrentTurnNumber => _currentTurnNumber; // Текущий номер хода для UI.
        /// <summary>Сервис выбора объекта.</summary>
        public SelectedService SelectedService => _selectedService;

        public StarSys? GetSelectedSystem()
        {
            return _selectedService.SelectedSystemService.GetSelectedSystem(_galaxy); // Безопасно вернуть выбранную систему или null.
        }

        public StarSys? GetActiveLocalSystem()
        {
            if (_galaxy == null || _galaxy.Length == 0)
                return null;
            if (_activeLocalSystemIndex < 0 || _activeLocalSystemIndex >= _galaxy.Length)
                return null;

            return _galaxy[_activeLocalSystemIndex];
        }

        public void SetRunMode(ERunMode mode)
        {
            if (_runMode == mode)
                return; // Не шлем лишние уведомления при том же режиме.

            _runMode = mode;
            NotifyChanged(); // Уведомляем подписчиков о смене режима.
        }

        public void SetGalaxy(StarSys[] galaxy)
        {
            _galaxy = galaxy ?? Array.Empty<StarSys>(); // Обновляем базовый state галактики.
            _selectedService.SelectedSystemService.OnGalaxySet(_galaxy); // Нормализуем текущий selected index.
            if (_activeLocalSystemIndex >= _galaxy.Length)
                _activeLocalSystemIndex = -1; // Если галактика пересоздана и индекс вышел за границы.
            _hyperlinkEdges = ConstellationCreator.BuildHyperlinkEdges(_galaxy); // Пересчитываем граф переходов.
            ContinuumService.Instance?.EnsureZones(this); // Перестраиваем зоны континуума под новый граф.
            NotifyChanged(); // Отдаем единое уведомление о смене state.
        }

        internal void SetConstellationList(int[][] list)
        {
            _constellationList = list ?? Array.Empty<int[]>(); // Обновляем кэш созвездий.
        }

        internal void ApplyShowHyperlinks(bool show)
        {
            if (_showHyperlinks == show)
                return; // Игнорируем noop.

            _showHyperlinks = show;
            NotifyChanged(); // Уведомляем UI.
        }

        internal void ApplyUseHyperlinkColoring(bool use)
        {
            if (_useHyperlinkColoring == use)
                return; // Игнорируем noop.

            _useHyperlinkColoring = use;
            NotifyChanged(); // Уведомляем UI.
        }

        internal void ApplyUseFractionColoring(bool use)
        {
            if (_useFractionColoring == use)
                return; // Игнорируем noop.

            _useFractionColoring = use;
            NotifyChanged(); // Уведомляем UI.
        }

        internal void ApplyUseSecurityColoring(bool use)
        {
            if (_useSecurityColoring == use)
                return; // Игнорируем noop.

            _useSecurityColoring = use;
            NotifyChanged(); // Уведомляем UI.
        }

        internal void ApplyCurrentTurnNumber(int currentTurnNumber)
        {
            if (currentTurnNumber < 0)
                currentTurnNumber = 0; // Негативный номер хода в UI не допускаем.

            if (_currentTurnNumber == currentTurnNumber)
                return; // Игнорируем noop.

            _currentTurnNumber = currentTurnNumber;
            NotifyChanged(); // Уведомляем UI о смене номера хода.
        }

        public bool SelectSystemByIndex(int index)
        {
            GlobalSyncDebugLog.Log("GameState", "select-by-index request target=" + index + " current=" + SelectedSystemIndex + " mode=" + _runMode);
            if (!PauseAndWaitGlobalForSelection())
                return false; // Не прошли handshake с global worker.

            var success = _selectedService.SelectedSystemService.SelectSystemByIndex(index, _galaxy, out var changed); // Применяем выбор.
            if (changed)
                NotifyChanged(); // Уведомляем UI о смене selected системы.

            GlobalSyncDebugLog.Log("GameState", "select-by-index applied target=" + index + " success=" + success + " changed=" + changed + " selected=" + SelectedSystemIndex);
            RestoreRunModeAfterSelection(); // Возвращаем run mode после handshake.
            return success;
        }

        public bool SelectSystemByUid(UID uid)
        {
            GlobalSyncDebugLog.Log("GameState", "select-by-uid request uid=" + uid.Id + " current=" + SelectedSystemIndex + " mode=" + _runMode);
            if (!PauseAndWaitGlobalForSelection())
                return false; // Не прошли handshake с global worker.

            var success = _selectedService.SelectedSystemService.SelectSystemByUid(uid, _galaxy, out var changed); // Применяем выбор по UID.
            if (changed)
                NotifyChanged(); // Уведомляем UI о смене selected системы.

            GlobalSyncDebugLog.Log("GameState", "select-by-uid applied uid=" + uid.Id + " success=" + success + " changed=" + changed + " selected=" + SelectedSystemIndex);
            RestoreRunModeAfterSelection(); // Возвращаем run mode после handshake.
            return success;
        }

        public void ClearSelectedSystem()
        {
            GlobalSyncDebugLog.Log("GameState", "clear-selected request current=" + SelectedSystemIndex + " mode=" + _runMode);
            if (!PauseAndWaitGlobalForSelection())
                return; // Не прошли handshake с global worker.

            if (_selectedService.SelectedSystemService.ClearSelectedSystem())
                NotifyChanged(); // Уведомляем UI о сбросе selected системы.

            GlobalSyncDebugLog.Log("GameState", "clear-selected applied selected=" + SelectedSystemIndex);
            RestoreRunModeAfterSelection(); // Возвращаем run mode после handshake.
        }

        public bool ActivateLocalFromSelectedSystem()
        {
            int selected = SelectedSystemIndex;
            if (selected < 0 || selected >= _galaxy.Length)
                return false; // В UI ничего не выбрано, локал активировать нельзя.

            return SetActiveLocalSystemIndex(selected);
        }

        public bool DeactivateLocalSystem()
        {
            return SetActiveLocalSystemIndex(-1);
        }

        internal void SetGlobalIdleWaiter(Func<bool> waitGlobalIdle)
        {
            _waitGlobalIdle = waitGlobalIdle; // Инжект из SimulationRootController.
        }

        private void NotifyChanged()
        {
            StateChanged?.Invoke(); // Единая точка рассылки уведомлений.
        }

        private bool SetActiveLocalSystemIndex(int index)
        {
            if (index < -1)
                return false;
            if (index >= _galaxy.Length)
                return false;
            if (_activeLocalSystemIndex == index)
                return true; // Уже в нужном состоянии.

            GlobalSyncDebugLog.Log("GameState", "set-active-local request target=" + index + " current=" + _activeLocalSystemIndex + " mode=" + _runMode);
            if (!PauseAndWaitGlobalForSelection())
                return false; // Границу переключения держим только при idle global worker.

            _activeLocalSystemIndex = index;
            NotifyChanged();
            GlobalSyncDebugLog.Log("GameState", "set-active-local applied active=" + _activeLocalSystemIndex + " selected=" + SelectedSystemIndex);
            RestoreRunModeAfterSelection();
            return true;
        }

        private bool PauseAndWaitGlobalForSelection()
        {
            if (_selectionHandshakeActive)
                return true; // Уже внутри handshake, повторно не стартуем.

            _runModeBeforeSelection = _runMode; // Запоминаем режим до handshake.
            if (_runMode != ERunMode.Paused)
                SetRunMode(ERunMode.Paused); // Смена активной системы допустима только из паузы.

            GlobalSyncDebugLog.Log("GameState", "handshake start runModeBefore=" + _runModeBeforeSelection + " current=" + _runMode);
            _selectionHandshakeActive = true; // Помечаем начало handshake.
            if (_waitGlobalIdle == null)
            {
                GlobalSyncDebugLog.Log("GameState", "handshake skip-wait (worker waiter is null)");
                return true; // В тестовом контуре без worker просто продолжаем.
            }

            bool idle = _waitGlobalIdle.Invoke();
            GlobalSyncDebugLog.Log("GameState", "handshake wait-global-idle result=" + idle);
            if (idle)
                return true; // Global worker подтвердил idle.

            _selectionHandshakeActive = false; // Сбрасываем флаг при таймауте ожидания.
            if (_runModeBeforeSelection == ERunMode.Auto && _runMode == ERunMode.Paused)
                SetRunMode(ERunMode.Auto); // Не удалось дождаться idle — возвращаем исходный режим.
            GlobalSyncDebugLog.Log("GameState", "handshake failed; runMode restored to " + _runMode);
            return false;
        }

        private void RestoreRunModeAfterSelection()
        {
            if (!_selectionHandshakeActive)
                return; // Вызвано вне handshake — ничего не делаем.

            _selectionHandshakeActive = false; // Завершаем handshake.
            if (_runModeBeforeSelection == ERunMode.Auto && _runMode == ERunMode.Paused)
                SetRunMode(ERunMode.Auto); // Возвращаем автопрогон после безопасной смены системы.
            GlobalSyncDebugLog.Log("GameState", "handshake done; runMode=" + _runMode + " selected=" + SelectedSystemIndex);
        }
    }
}
