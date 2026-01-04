using System;
using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Galaxy.Generation;
using _Project.Scripts.Core.GameState.GameStateMembers;

namespace _Project.Scripts.Core.GameState
{
    /// <summary>Минимальное состояние игры: режим, галактика и выбранная система.</summary>
    public sealed class GameStateService
    {
        private StarSys[] _galaxy = Array.Empty<StarSys>();
        private HyperlinkEdge[] _hyperlinkEdges = Array.Empty<HyperlinkEdge>();
        private int[][] _constellationList = Array.Empty<int[]>();
        private ERunMode _runMode = ERunMode.Paused;
        private readonly SelectedService _selectedService = new SelectedService();
        private bool _showHyperlinks = true;
        private bool _useHyperlinkColoring = true;
        private bool _useFractionColoring = true;

        public event Action StateChanged; // Уведомление для UI/логики о смене состояния.

        public GameStateService()
        {
            _showHyperlinks = SettingsService.Instance.ShowHyperlinks;
            _useHyperlinkColoring = SettingsService.Instance.UseHyperlinkColoring;
            _useFractionColoring = SettingsService.Instance.UseFractionColoring;
        }

        public ERunMode RunMode => _runMode;
        public StarSys[] Galaxy => _galaxy;
        public HyperlinkEdge[] HyperlinkEdges => _hyperlinkEdges;
        public int[][] ConstellationList => _constellationList;
        public int SelectedSystemIndex => _selectedService.SelectedSystemService.SelectedSystemIndex;
        public bool ShowHyperlinks => _showHyperlinks;
        public bool UseHyperlinkColoring => _useHyperlinkColoring;
        public bool UseFractionColoring => _useFractionColoring;
        /// <summary>Сервис выбора объекта.</summary>
        public SelectedService SelectedService => _selectedService;

        public StarSys? GetSelectedSystem()
        {
            return _selectedService.SelectedSystemService.GetSelectedSystem(_galaxy);
        }

        public void SetRunMode(ERunMode mode)
        {
            if (_runMode == mode)
                return;

            _runMode = mode;
            NotifyChanged();
        }

        public void SetGalaxy(StarSys[] galaxy)
        {
            _galaxy = galaxy ?? Array.Empty<StarSys>();
            _selectedService.SelectedSystemService.OnGalaxySet(_galaxy);
            _hyperlinkEdges = ConstellationCreator.BuildHyperlinkEdges(_galaxy);
            NotifyChanged();
        }

        internal void SetConstellationList(int[][] list)
        {
            _constellationList = list ?? Array.Empty<int[]>();
        }

        internal void ApplyShowHyperlinks(bool show)
        {
            if (_showHyperlinks == show)
                return;

            _showHyperlinks = show;
            NotifyChanged();
        }

        internal void ApplyUseHyperlinkColoring(bool use)
        {
            if (_useHyperlinkColoring == use)
                return;

            _useHyperlinkColoring = use;
            NotifyChanged();
        }

        internal void ApplyUseFractionColoring(bool use)
        {
            if (_useFractionColoring == use)
                return;

            _useFractionColoring = use;
            NotifyChanged();
        }

        public bool SelectSystemByIndex(int index)
        {
            var success = _selectedService.SelectedSystemService.SelectSystemByIndex(index, _galaxy, out var changed);
            if (changed)
                NotifyChanged();

            return success;
        }

        public bool SelectSystemByUid(UID uid)
        {
            var success = _selectedService.SelectedSystemService.SelectSystemByUid(uid, _galaxy, out var changed);
            if (changed)
                NotifyChanged();

            return success;
        }

        public void ClearSelectedSystem()
        {
            if (_selectedService.SelectedSystemService.ClearSelectedSystem())
                NotifyChanged();
        }

        private void NotifyChanged()
        {
            StateChanged?.Invoke();
        }
    }
}
