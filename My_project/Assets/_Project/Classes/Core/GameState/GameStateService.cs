using System;
using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Core.GameState.GameStateMembers.SelectedObj;

namespace _Project.Scripts.Core.GameState
{
    /// <summary>Минимальное состояние игры: режим, галактика и выбранная система.</summary>
    public sealed class GameStateService
    {
        private StarSys[] _galaxy = Array.Empty<StarSys>();
        private int _selectedSystemIndex = -1;
        private ERunMode _runMode = ERunMode.Paused;
        private readonly SelectedObjectService _selectedObjectService = new SelectedObjectService();

        public event Action StateChanged; // Уведомление для UI/логики о смене состояния.

        public GameStateService()
        {
        }

        public ERunMode RunMode => _runMode;
        public StarSys[] Galaxy => _galaxy;
        public int SelectedSystemIndex => _selectedSystemIndex;
        /// <summary>Сервис выделенного объекта.</summary>
        public SelectedObjectService SelectedObjectService => _selectedObjectService;

        public StarSys? GetSelectedSystem()
        {
            if (_galaxy == null || _galaxy.Length == 0)
                return null;

            if (_selectedSystemIndex < 0 || _selectedSystemIndex >= _galaxy.Length)
                return null;

            return _galaxy[_selectedSystemIndex];
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

            if (_galaxy.Length == 0)
                _selectedSystemIndex = -1;
            else if (_selectedSystemIndex < 0 || _selectedSystemIndex >= _galaxy.Length)
                _selectedSystemIndex = 0;

            NotifyChanged();
        }

        public bool SelectSystemByIndex(int index)
        {
            if (_galaxy == null || _galaxy.Length == 0)
            {
                _selectedSystemIndex = -1;
                NotifyChanged();
                return false;
            }

            if (index < 0)
                index = 0;
            else if (index >= _galaxy.Length)
                index = _galaxy.Length - 1;

            if (_selectedSystemIndex == index)
                return true;

            _selectedSystemIndex = index;
            NotifyChanged();
            return true;
        }

        public bool SelectSystemByUid(UID uid)
        {
            var galaxy = _galaxy;
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

        public void ClearSelectedSystem()
        {
            if (_selectedSystemIndex == -1)
                return;

            _selectedSystemIndex = -1;
            NotifyChanged();
        }

        private void NotifyChanged()
        {
            StateChanged?.Invoke();
        }
    }
}
