using System;
using UnityEngine;

namespace _Project.Items
{
    [Serializable]
    public struct ArtifactData
    {
        [field: SerializeField] public string EffectId { get; private set; } // например, ключ для системы эффектов
        [field: SerializeField] public float Power { get; private set; }
        [field: SerializeField] public float Cooldown { get; private set; }
    }
}
