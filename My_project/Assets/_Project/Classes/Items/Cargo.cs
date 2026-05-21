using System.Collections.Generic;
namespace _Project.Items
{
    /// <summary>Единое хранилище грузов для кораблей, станций и т.д.</summary>
    public sealed class Cargo
    {
        private readonly Dictionary<ItemKey, int> _stock = new();

        public Cargo(int capacity = 0)
        {
            Capacity = capacity;
        }

        public int Capacity { get; set; } // 0 или меньше = безлимит

        public IReadOnlyDictionary<ItemKey, int> Stock => _stock;

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

        public int GetAmount(ItemKey key)
        {
            return _stock.TryGetValue(key, out var amount) ? amount : 0;
        }

        public bool CanAdd(ItemKey key, int amount)
        {
            if (amount <= 0)
                return true;
            if (Capacity <= 0)
                return true;

            return Used + amount <= Capacity;
        }

        public void Add(ItemKey key, int amount)
        {
            if (amount <= 0)
                return;
            if (key.IsEmpty)
                return;
            if (!CanAdd(key, amount))
                return;

            if (_stock.TryGetValue(key, out var current))
                _stock[key] = current + amount;
            else
                _stock[key] = amount;
        }

        public void Remove(ItemKey key, int amount)
        {
            if (amount <= 0)
                return;
            if (!_stock.TryGetValue(key, out var current))
                return;

            var next = current - amount;
            if (next > 0)
                _stock[key] = next;
            else
                _stock.Remove(key);
        }

        public void SetAmount(ItemKey key, int amount)
        {
            if (amount <= 0)
                _stock.Remove(key);
            else if (!key.IsEmpty)
                _stock[key] = amount;
        }

        public bool Contains(ItemKey key)
        {
            return _stock.ContainsKey(key);
        }

    }
}
