using System.Collections.Generic;

namespace _Project.Scripts.Simulation.AI
{
    /// <summary>Нейтральное хранилище активных и временно прерванных поведений.</summary>
    public sealed class ShipAiBehaviorStack
    {
        private readonly Stack<ShipAiBehavior> _items = new Stack<ShipAiBehavior>();

        public bool IsEmpty => _items.Count == 0;

        public void Push(ShipAiBehavior behavior)
        {
            _items.Push(behavior);
        }

        public bool TryPeek(out ShipAiBehavior behavior)
        {
            if (_items.Count == 0)
            {
                behavior = null;
                return false;
            }

            behavior = _items.Peek();
            return true;
        }

        public void Pop()
        {
            if (_items.Count > 0)
                _items.Pop();
        }

        public void Clear()
        {
            _items.Clear();
        }
    }
}
