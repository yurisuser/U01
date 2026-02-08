using System;
using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Galaxy.Generation;
using _Project.Scripts.Simulation.Continuum;
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
        private bool _showHyperlinks = true; // Флаг показа гиперлинков в представлении.
        private bool _useHyperlinkColoring = true; // Флаг раскраски по гиперлинкам.
        private bool _useFractionColoring = true; // Флаг раскраски по фракциям.
        private Func<bool> _waitGlobalIdle; // Колбэк ожидания idle у global worker (handshake выбора системы).
        private ERunMode _runModeBeforeSelection = ERunMode.Paused; // Режим до входа в handshake выбора системы.
        private bool _selectionHandshakeActive; // Защита от повторного входа в handshake выбора системы.

        public event Action StateChanged; // Уведомление для UI/логики о смене состояния.

        public GameStateService()
        {
            _showHyperlinks = SettingsService.Instance.ShowHyperlinks;
            _useHyperlinkColoring = SettingsService.Instance.UseHyperlinkColoring;
            _useFractionColoring = SettingsService.Instance.UseFractionColoring;
        }

        public ERunMode RunMode => _runMode; // Публичный read-only доступ к текущему run mode.
        public StarSys[] Galaxy => _galaxy; // Публичный доступ к массиву систем.
        public HyperlinkEdge[] HyperlinkEdges => _hyperlinkEdges; // Публичный доступ к графу гиперсвязей.
        public int[][] ConstellationList => _constellationList; // Публичный доступ к списку созвездий.
        public int SelectedSystemIndex => _selectedService.SelectedSystemService.SelectedSystemIndex; // Индекс активной системы.
        public bool ShowHyperlinks => _showHyperlinks; // UI-флаг показа гиперлинков.
        public bool UseHyperlinkColoring => _useHyperlinkColoring; // UI-флаг раскраски по гиперлинкам.
        public bool UseFractionColoring => _useFractionColoring; // UI-флаг раскраски по фракциям.
        /// <summary>Сервис выбора объекта.</summary>
        public SelectedService SelectedService => _selectedService;

        public StarSys? GetSelectedSystem()
        {
            return _selectedService.SelectedSystemService.GetSelectedSystem(_galaxy); // Безопасно вернуть выбранную систему или null.
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

        public bool SelectSystemByIndex(int index)
        {
            if (!PauseAndWaitGlobalForSelection())
                return false; // Не прошли handshake с global worker.

            var success = _selectedService.SelectedSystemService.SelectSystemByIndex(index, _galaxy, out var changed); // Применяем выбор.
            if (changed)
                NotifyChanged(); // Уведомляем UI о смене selected системы.

            RestoreRunModeAfterSelection(); // Возвращаем run mode после handshake.
            return success;
        }

        public bool SelectSystemByUid(UID uid)
        {
            if (!PauseAndWaitGlobalForSelection())
                return false; // Не прошли handshake с global worker.

            var success = _selectedService.SelectedSystemService.SelectSystemByUid(uid, _galaxy, out var changed); // Применяем выбор по UID.
            if (changed)
                NotifyChanged(); // Уведомляем UI о смене selected системы.

            RestoreRunModeAfterSelection(); // Возвращаем run mode после handshake.
            return success;
        }

        public void ClearSelectedSystem()
        {
            if (!PauseAndWaitGlobalForSelection())
                return; // Не прошли handshake с global worker.

            if (_selectedService.SelectedSystemService.ClearSelectedSystem())
                NotifyChanged(); // Уведомляем UI о сбросе selected системы.

            RestoreRunModeAfterSelection(); // Возвращаем run mode после handshake.
        }

        internal void SetGlobalIdleWaiter(Func<bool> waitGlobalIdle)
        {
            _waitGlobalIdle = waitGlobalIdle; // Инжект из SimulationRootController.
        }

        private void NotifyChanged()
        {
            StateChanged?.Invoke(); // Единая точка рассылки уведомлений.
        }

        private bool PauseAndWaitGlobalForSelection()
        {
            if (_selectionHandshakeActive)
                return true; // Уже внутри handshake, повторно не стартуем.

            _runModeBeforeSelection = _runMode; // Запоминаем режим до handshake.
            if (_runMode != ERunMode.Paused)
                SetRunMode(ERunMode.Paused); // Смена активной системы допустима только из паузы.

            _selectionHandshakeActive = true; // Помечаем начало handshake.
            if (_waitGlobalIdle == null)
                return true; // В тестовом контуре без worker просто продолжаем.

            if (_waitGlobalIdle.Invoke())
                return true; // Global worker подтвердил idle.

            _selectionHandshakeActive = false; // Сбрасываем флаг при таймауте ожидания.
            if (_runModeBeforeSelection == ERunMode.Auto && _runMode == ERunMode.Paused)
                SetRunMode(ERunMode.Auto); // Не удалось дождаться idle — возвращаем исходный режим.
            return false;
        }

        private void RestoreRunModeAfterSelection()
        {
            if (!_selectionHandshakeActive)
                return; // Вызвано вне handshake — ничего не делаем.

            _selectionHandshakeActive = false; // Завершаем handshake.
            if (_runModeBeforeSelection == ERunMode.Auto && _runMode == ERunMode.Paused)
                SetRunMode(ERunMode.Auto); // Возвращаем автопрогон после безопасной смены системы.
        }
    }
}
