using System;
using UnityEngine;

namespace _Project.Items
{
    [Serializable]
    public struct WeaponData
    {
        [field: SerializeField] public float Damage { get; private set; }
        [field: SerializeField] public float RatePerSecond { get; private set; }
        [field: SerializeField] public float Range { get; private set; }
        [field: SerializeField] public StatModifier[] ExtraDamage { get; private set; }
    }
}
