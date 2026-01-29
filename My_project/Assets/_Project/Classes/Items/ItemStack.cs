using System;
using UnityEngine;

namespace _Project.Items
{
    [Serializable]
    public struct ItemStack
    {
        [field: SerializeField] public int Id { get; private set; }
        [field: SerializeField] public ItemType Type { get; private set; }
        [field: SerializeField] public int Quantity { get; private set; }

        public ItemStack(ItemType type, int id, int quantity)
        {
            Type = type;
            Id = id;
            Quantity = quantity;
        }
    }
}
