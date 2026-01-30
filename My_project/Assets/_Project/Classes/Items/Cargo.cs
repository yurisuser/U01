using System.Collections.Generic;
namespace _Project.Items
{
    /// <summary>Единое хранилище грузов для кораблей, станций и т.д.</summary>
    public sealed class Cargo
    {
        private readonly Dictionary<int, int> _stock = new();

        public Cargo(int capacity = 0)
        {
            Capacity = capacity;
        }

        public int Capacity { get; set; } // 0 или меньше = безлимит

        public IReadOnlyDictionary<int, int> Stock => _stock;

        public int Used
        {
            get
            {
                int total = 0;
                foreach (var pair in _stock)
                    total += pair.Value;
                return total;
            }
        }

        public int GetAmount(ItemType type, int itemId)
        {
            return _stock.TryGetValue(itemId, out var amount) ? amount : 0;
        }

        public bool CanAdd(ItemType type, int itemId, int amount)
        {
            if (amount <= 0)
                return true;
            if (Capacity <= 0)
                return true;

            return Used + amount <= Capacity;
        }

        public void Add(ItemType type, int itemId, int amount)
        {
            if (amount <= 0)
                return;
            if (!CanAdd(type, itemId, amount))
                return;

            if (_stock.TryGetValue(itemId, out var current))
                _stock[itemId] = current + amount;
            else
                _stock[itemId] = amount;
        }

        public void Remove(ItemType type, int itemId, int amount)
        {
            if (amount <= 0)
                return;
            if (!_stock.TryGetValue(itemId, out var current))
                return;

            var next = current - amount;
            if (next > 0)
                _stock[itemId] = next;
            else
                _stock.Remove(itemId);
        }

        public void SetAmount(int itemId, int amount)
        {
            if (amount <= 0)
                _stock.Remove(itemId);
            else
                _stock[itemId] = amount;
        }
    }
}
