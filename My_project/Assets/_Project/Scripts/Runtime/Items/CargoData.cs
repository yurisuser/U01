using System;
using UnityEngine;

namespace _Project.Items
{
    [Serializable]
    public struct CargoData
    {
        [field: SerializeField] public int Volume { get; private set; }
        [field: SerializeField] public int Value { get; private set; }
    }
}
