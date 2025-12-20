using _Project.Scripts.Core;

namespace _Project.Scripts.Core.GameState.GameStateMembers.SelectedObj
{
    /// <summary>Минимальные данные о выделенном объекте на карте.</summary>
    public readonly struct SelectedObject
    {
        public SelectedObject(int systemIndex, UID uid, ESelectedObjectType type) //конструктор
        {
            SysIndex = systemIndex;
            UID = uid;
            Type = type;
        }

        /// <summary>Индекс системы, в которой был выбран объект.</summary>
        public int SysIndex { get; }

        /// <summary>UID выбранной сущности.</summary>
        public UID UID { get; }

        /// <summary>Тип выделенного объекта.</summary>
        public ESelectedObjectType Type { get; }
    }
}
