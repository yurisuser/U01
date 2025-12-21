using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState.GameStateMembers.SelectedObj;
using _Project.Scripts.Core.GameState.GameStateMembers.SelectedSystem;

namespace _Project.Scripts.Core.GameState.GameStateMembers
{
    /// <summary>Главный сервис выделения (система + объект).</summary>
    public sealed class SelectedService
    {
        private readonly SelectedSystemService _selectedSystemService = new SelectedSystemService();
        private readonly SelectedObjectService _selectedObjectService = new SelectedObjectService();

        /// <summary>Дочерний сервис выделенной системы.</summary>
        public SelectedSystemService SelectedSystemService => _selectedSystemService;
        /// <summary>Дочерний сервис выделенного объекта.</summary>
        public SelectedObjectService SelectedObjectService => _selectedObjectService;

        public void SetSelected(int systemIndex, UID uid, ESelectedObjectType type)
        {
            var selection = new SelectedObject(systemIndex, uid, type);
            _selectedObjectService.SetSelectedObject(selection);
        }

        public void ClearSelected()
        {
            _selectedObjectService.SetSelectedObject(null);
        }
    }
}
