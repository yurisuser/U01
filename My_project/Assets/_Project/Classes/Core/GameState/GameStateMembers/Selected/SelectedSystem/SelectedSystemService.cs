using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;

namespace _Project.Scripts.Core.GameState.GameStateMembers.SelectedSystem
{
    /// <summary>Сервис, управляющий выбранной системой в галактике.</summary>
    public sealed class SelectedSystemService
    {
        private int _selectedSystemIndex = -1;

        public int SelectedSystemIndex => _selectedSystemIndex;

        /// <summary>Проверяет актуальность выбора после смены галактики.</summary>
        public bool OnGalaxySet(StarSys[] galaxy)
        {
            int newIndex = ResolveIndexWithinGalaxy(galaxy);
            if (_selectedSystemIndex == newIndex)
                return false;

            _selectedSystemIndex = newIndex;
            return true;
        }

        public StarSys? GetSelectedSystem(StarSys[] galaxy)
        {
            if (galaxy == null || galaxy.Length == 0)
                return null;

            if (_selectedSystemIndex < 0 || _selectedSystemIndex >= galaxy.Length)
                return null;

            return galaxy[_selectedSystemIndex];
        }

        public bool SelectSystemByIndex(int index, StarSys[] galaxy, out bool changed)
        {
            changed = false;
            if (galaxy == null || galaxy.Length == 0)
            {
                changed = ClearInternal();
                return false;
            }

            if (index < 0)
                index = 0;
            else if (index >= galaxy.Length)
                index = galaxy.Length - 1;

            if (_selectedSystemIndex == index)
                return true;

            _selectedSystemIndex = index;
            changed = true;
            return true;
        }

        public bool SelectSystemByUid(UID uid, StarSys[] galaxy, out bool changed)
        {
            changed = false;
            if (galaxy == null || galaxy.Length == 0)
            {
                changed = ClearInternal();
                return false;
            }

            for (int i = 0; i < galaxy.Length; i++)
            {
                var sys = galaxy[i];
                if (sys.Uid.Type == uid.Type && sys.Uid.Id == uid.Id)
                    return SelectSystemByIndex(i, galaxy, out changed);
            }

            return false;
        }

        public bool ClearSelectedSystem()
        {
            return ClearInternal();
        }

        private bool ClearInternal()
        {
            if (_selectedSystemIndex == -1)
                return false;

            _selectedSystemIndex = -1;
            return true;
        }

        private int ResolveIndexWithinGalaxy(StarSys[] galaxy)
        {
            if (galaxy == null || galaxy.Length == 0)
                return -1;

            if (_selectedSystemIndex < 0 || _selectedSystemIndex >= galaxy.Length)
                return -1;

            return _selectedSystemIndex;
        }
    }
}
