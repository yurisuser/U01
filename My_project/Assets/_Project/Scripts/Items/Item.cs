using System;
using UnityEngine;

namespace _Project.Items
{
    [Serializable]
    public struct Item // Базовая модель предмета инвентаря
    {
        [field: SerializeField] public string Id { get; private set; } // Уникальный строковый идентификатор предмета (ID)
        [field: SerializeField] public string DisplayName { get; private set; } // Отображаемое имя для UI
        [field: SerializeField] public Sprite Icon { get; private set; } // Иконка предмета
        [field: SerializeField] public ItemType Item_Type { get; private set; } // Тип предмета, определяет активный пэйлоад
        [field: SerializeField] public ItemRarity Rarity { get; private set; } // Редкость предмета
        [field: SerializeField] public bool Stackable { get; private set; } // Можно ли складывать в стопку
        [field: SerializeField] public int MaxStack { get; private set; }  // Макс. размер стопки (1 для оружия/экипа)
        [field: SerializeField] public float Weight { get; private set; } // Масса одного экземпляра
        [field: SerializeField] public StatModifier[] UniversalStats { get; private set; } // Универсальные модификаторы статов

        [field: SerializeField] public ItemStack Cargo { get; private set; } // Пэйлоад для Cargo (актуален при соответствующем Item_Type)
        [field: SerializeField] public WeaponData Weapon { get; private set; } // Пэйлоад для Weapon (актуален при соответствующем Item_Type)
        [field: SerializeField] public EquipmentData Equipment { get; private set; } // Пэйлоад для Equipment (актуален при соответствующем Item_Type)
        [field: SerializeField] public ArtifactData Artifact { get; private set; } // Пэйлоад для Artifact (актуален при соответствующем Item_Type)

        private bool TryGetPayload<T>(ItemType expected, T payload, out T result) // true, если тип совпал; result = payload
        {
            result = payload;
            return Item_Type == expected;
        }
    }
}
