using System;
using UnityEngine;

namespace _Project.Items
{
    [Serializable]
    public struct Item
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public ItemType Item_Type { get; private set; }
        [field: SerializeField] public ItemRarity Rarity { get; private set; }
        [field: SerializeField] public bool Stackable { get; private set; }
        [field: SerializeField] public int MaxStack { get; private set; }  // 1 для оружия/экипа
        [field: SerializeField] public float Weight { get; private set; }
        [field: SerializeField] public StatModifier[] UniversalStats { get; private set; }

        [field: SerializeField] public CargoData Cargo { get; private set; }
        [field: SerializeField] public WeaponData Weapon { get; private set; }
        [field: SerializeField] public EquipmentData Equipment { get; private set; }
        [field: SerializeField] public ArtifactData Artifact { get; private set; }

        private bool TryGetPayload<T>(ItemType expected, T payload, out T result)
        {
            result = payload;
            return Item_Type == expected;
        }
    }
}
