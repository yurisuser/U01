using System;

namespace _Project.Scripts.Simulation.PilotMotivation
{
    /// <summary>
    /// Value-type stack wrapper for pilot actions that avoids direct dependency on Stack{T}.
    /// </summary>
    public struct PilotActionStack
    {
        private PilotAction[] _buffer;
        private int _count;

        private const int DefaultCapacity = 16;

        public int Count => _count;
        public bool IsCreated => _buffer != null;

        public void Initialize(int initialCapacity = DefaultCapacity)
        {
            if (initialCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(initialCapacity), "Capacity must be non-negative.");

            var capacity = initialCapacity == 0 ? 0 : Math.Max(initialCapacity, DefaultCapacity);

            if (_buffer == null || _buffer.Length < capacity)
                _buffer = capacity == 0 ? Array.Empty<PilotAction>() : new PilotAction[capacity];
            else
                Array.Clear(_buffer, 0, _count);

            _count = 0;
        }

        public PilotActionStack(int initialCapacity)
        {
            if (initialCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(initialCapacity), "Capacity must be non-negative.");

            var capacity = initialCapacity == 0 ? 0 : Math.Max(initialCapacity, DefaultCapacity);
            _buffer = capacity == 0 ? Array.Empty<PilotAction>() : new PilotAction[capacity];
            _count = 0;
        }

        public void Push(in PilotAction action)
        {
            EnsureCapacity(_count + 1);
            _buffer[_count++] = action;
        }

        public void ReplaceTop(in PilotAction action)
        {
            if (_count == 0)
                throw new InvalidOperationException("Cannot replace top element of an empty stack.");

            _buffer[_count - 1] = action;
        }

        public PilotAction Pop()
        {
            if (_count == 0)
                throw new InvalidOperationException("Cannot pop from an empty stack.");

            var index = --_count;
            var action = _buffer[index];
            _buffer[index] = default;
            return action;
        }

        public bool TryPop(out PilotAction action)
        {
            if (_count == 0)
            {
                action = default;
                return false;
            }

            var index = --_count;
            action = _buffer[index];
            _buffer[index] = default;
            return true;
        }

        public ref readonly PilotAction Peek()
        {
            if (_count == 0)
                throw new InvalidOperationException("Cannot peek an empty stack.");

            return ref _buffer[_count - 1];
        }

        public bool TryPeek(out PilotAction action)
        {
            if (_count == 0)
            {
                action = default;
                return false;
            }

            action = _buffer[_count - 1];
            return true;
        }

        public void Clear()
        {
            if (_count == 0)
                return;

            if (_buffer != null)
                Array.Clear(_buffer, 0, _count);

            _count = 0;
        }

        public ReadOnlySpan<PilotAction> AsReadOnlySpan()
        {
            return new ReadOnlySpan<PilotAction>(_buffer, 0, _count);
        }

        private void EnsureCapacity(int size)
        {
            if (_buffer == null || _buffer.Length == 0)
            {
                var capacity = Math.Max(size, DefaultCapacity);
                _buffer = new PilotAction[capacity];
                return;
            }

            if (_buffer.Length >= size)
                return;

            var newCapacity = _buffer.Length * 2;
            while (newCapacity < size)
                newCapacity *= 2;

            Array.Resize(ref _buffer, newCapacity);
        }
    }
}
