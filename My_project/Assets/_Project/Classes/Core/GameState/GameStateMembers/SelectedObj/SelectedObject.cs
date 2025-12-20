using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState.GameStateMembers.SelectedObj;

namespace _Project.Scripts.Core.GameState.GameStateMembers.SelectedObj
{
    /// <summary>Минимальные данные о выделенном объекте на карте.</summary>
    public readonly struct SelectedObject
    {
        public SelectedObject(SelectedObjectType type, UID targetId, int systemIndex, int version)
        {
            Type = type;
            TargetId = targetId;
            SystemIndex = systemIndex;
            Version = version;
        }

        /// <summary>К какому виду сущностей относится выделение.</summary>
        public SelectedObjectType Type { get; }

        /// <summary>UID выделенного объекта.</summary>
        public UID TargetId { get; }

        /// <summary>Индекс системы, в контексте которой был выбран объект.</summary>
        public int SystemIndex { get; }

        /// <summary>Версия селектора; используется для автоматического сброса.</summary>
        public int Version { get; }
    }
}
