using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState.GameStateMembers.SelectedObj;
using UnityEngine;

namespace _Project.Scripts.Selection
{
    /// <summary>Компонент-хранилище данных для выбора объекта.</summary>
    public sealed class SelectableData : MonoBehaviour
    {
        [SerializeField] private int systemIndex = -1;
        [SerializeField] private EntityType entityType = EntityType.None;
        [SerializeField] private int uidId = -1;
        [SerializeField] private ESelectedObjectType selectedType = ESelectedObjectType.None;
        [SerializeField] private bool hasData;

        public bool HasData => hasData;
        public int SystemIndex => systemIndex;
        public UID Uid => new UID(entityType, uidId);
        public ESelectedObjectType SelectedType => selectedType;

        public void SetData(int systemIndex, UID uid, ESelectedObjectType selectedType)
        {
            this.systemIndex = systemIndex;
            entityType = uid.Type;
            uidId = uid.Id;
            this.selectedType = selectedType;
            hasData = true;
        }
    }
}
