namespace _Project.Scripts.Core.GameState.GameStateMembers.SelectedObj
{
    /// <summary>Сервис хранения текущего выделенного объекта.</summary>
    public sealed class SelectedObjectService
    {
        private SelectedObject? _current;

        public SelectedObject? GetSelectedObject() => _current;

        public void SetSelectedObject(SelectedObject? selection) => _current = selection;
    }
}
