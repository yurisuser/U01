using System;
using UnityEngine;

namespace _Project.Items
{
    [Serializable]
    public struct StatModifier
    {
        [field: SerializeField] public string StatId { get; private set; }
        [field: SerializeField] public float Value { get; private set; }
    }
}
