using System;
using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Core.GameState.GameStateMembers.SelectedObj;
using _Project.Scripts.Core.GameState.GameStateMembers.SelectedSystem;

namespace _Project.Scripts.Core.GameState
{
    /// <summary>Минимальное состояние игры: режим, галактика и выбранная система.</summary>
    public sealed class GameStateService
    {
        private StarSys[] _galaxy = Array.Empty<StarSys>();
        private ERunMode _runMode = ERunMode.Paused;
        private readonly SelectedSystemService _selectedSystemService = new SelectedSystemService();
        private readonly SelectedObjectService _selectedObjectService = new SelectedObjectService();

        public event Action StateChanged; // Уведомление для UI/логики о смене состояния.

        public GameStateService()
        {
        }

        public ERunMode RunMode => _runMode;
        public StarSys[] Galaxy => _galaxy;
        public int SelectedSystemIndex => _selectedSystemService.SelectedSystemIndex;
        /// <summary>Сервис выделенного объекта.</summary>
        public SelectedObjectService SelectedObjectService => _selectedObjectService;

        public StarSys? GetSelectedSystem()
        {
            return _selectedSystemService.GetSelectedSystem(_galaxy);
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
            _selectedSystemService.OnGalaxySet(_galaxy);
            NotifyChanged();
        }

        public bool SelectSystemByIndex(int index)
        {
            var success = _selectedSystemService.SelectSystemByIndex(index, _galaxy, out var changed);
            if (changed)
                NotifyChanged();

            return success;
        }

        public bool SelectSystemByUid(UID uid)
        {
            var success = _selectedSystemService.SelectSystemByUid(uid, _galaxy, out var changed);
            if (changed)
                NotifyChanged();

            return success;
        }

        public void ClearSelectedSystem()
        {
            if (_selectedSystemService.ClearSelectedSystem())
                NotifyChanged();
        }

        private void NotifyChanged()
        {
            StateChanged?.Invoke();
        }
    }
}
