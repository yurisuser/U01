using System;
using UnityEngine;

namespace _Project.Items
{
    [Serializable]
    public struct EquipmentData
    {
        //[field: SerializeField] public EquipmentSlot Slot { get; private set; }
        [field: SerializeField] public StatModifier[] Bonuses { get; private set; }
    }
}
