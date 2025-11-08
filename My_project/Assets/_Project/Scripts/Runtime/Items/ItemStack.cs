using System;
using UnityEngine;

namespace _Project.Items
{
    [Serializable]
    public struct ItemStack
    {
        [field: SerializeField] public int Id { get; private set; }
        [field: SerializeField] public int Amount { get; private set; }
    }
}
