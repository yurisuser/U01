using System;
using UnityEngine;

namespace _Project.Items
{
    /// <summary>Уникальный ключ перевозимого предмета: тип каталога + id внутри таблицы этого типа.</summary>
    [Serializable]
    public struct ItemKey : IEquatable<ItemKey>
    {
        [field: SerializeField] public ItemType Type { get; private set; }
        [field: SerializeField] public int Id { get; private set; }

        public ItemKey(ItemType type, int id)
        {
            Type = type;
            Id = id;
        }

        public bool IsEmpty => Type == ItemType.None || Id <= 0;

        public bool Equals(ItemKey other)
        {
            return Type == other.Type && Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is ItemKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Type * 397) ^ Id;
            }
        }

        public override string ToString()
        {
            return $"{Type}:{Id}";
        }

        public static bool operator ==(ItemKey left, ItemKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ItemKey left, ItemKey right)
        {
            return !left.Equals(right);
        }
    }
}
