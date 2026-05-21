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
        public ItemKey Key => new ItemKey(Type, Id);

        public ItemStack(ItemKey key, int quantity)
        {
            Type = key.Type;
            Id = key.Id;
            Quantity = quantity;
        }
    }
}
