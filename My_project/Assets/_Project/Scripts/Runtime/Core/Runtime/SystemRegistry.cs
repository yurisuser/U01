using System;
using _Project.Scripts.Ships;

namespace _Project.Scripts.Core.Runtime
{
    /// <summary>Хранилище состояния всех звёздных систем в рантайме (включая корабли).</summary>
    public sealed class SystemRegistry
    {
        private StarSystemState[] _systems = Array.Empty<StarSystemState>(); // Список состояний систем.

        /// <summary>Сколько систем обслуживается.</summary>
        public int Count => _systems.Length; // Сколько систем обслуживается.

        /// <summary>Готовит массив состояний на основе данных галактики.</summary>
        public void Initialize(GalaxyService galaxy)
        {
            Reset();

            var total = galaxy?.Count ?? 0;
            if (total <= 0)
            {
                _systems = Array.Empty<StarSystemState>();
                return;
            }

            _systems = new StarSystemState[total];
            for (int i = 0; i < total; i++)
                _systems[i] = new StarSystemState();
        }

        /// <summary>Полностью очищает все состояния систем.</summary>
        public void Reset()
        {
            if (_systems == null)
                return;

            for (int i = 0; i < _systems.Length; i++)
                _systems[i]?.Reset();

            _systems = Array.Empty<StarSystemState>();
        }

        /// <summary>Пытается получить состояние системы по индексу.</summary>
        public bool TryGetState(int systemId, out StarSystemState state)
        {
            if ((uint)systemId < _systems.Length)
            {
                state = _systems[systemId];
                return state != null;
            }

            state = null;
            return false;
        }

        /// <summary>Добавляет корабль в конкретную систему и возвращает слот.</summary>
        public int AddShip(int systemId, in Ship ship)
        {
            if (!TryGetState(systemId, out var state))
                throw new ArgumentOutOfRangeException(nameof(systemId));

            return state.AddShip(in ship);
        }

        /// <summary>Пытается получить корабль по слоту системы.</summary>
        public bool TryGetShip(int systemId, int slot, out Ship ship)
        {
            ship = default;
            return TryGetState(systemId, out var state) && state.TryGetShip(slot, out ship);
        }

        /// <summary>Обновляет данные корабля в слоте.</summary>
        public bool TryUpdateShip(int systemId, int slot, in Ship ship)
        {
            return TryGetState(systemId, out var state) && state.TryUpdateShip(slot, in ship);
        }

        /// <summary>Удаляет корабль из системы.</summary>
        public bool TryRemoveShip(int systemId, int slot, out Ship ship)
        {
            ship = default;
            return TryGetState(systemId, out var state) && state.TryRemoveShip(slot, out ship);
        }

        /// <summary>Копирует корабли системы в пользовательский буфер (для UI).</summary>
        public int CopyShipsToBuffer(int systemId, ref Ship[] buffer)
        {
            if (!TryGetState(systemId, out var state))
                return 0;

            return state.CopyShips(ref buffer);
        }
    }

    /// <summary>Состояние кораблей внутри одной звёздной системы.</summary>
    public sealed class StarSystemState
    {
        private Ship[] _ships = Array.Empty<Ship>(); // Буфер слотов.
        private int _shipCount; // Активные слоты.

        /// <summary>Сколько слотов занято.</summary>
        public int ShipCount => _shipCount; // Сколько слотов занято.
        /// <summary>Прямой доступ к буферу кораблей (использовать с ShipCount).</summary>
        public Ship[] ShipsBuffer => _ships; // Прямой доступ к буферу (использовать с ShipCount).

        /// <summary>Добавляет корабль, расширяя буфер по мере необходимости.</summary>
        public int AddShip(in Ship ship)
        {
            EnsureCapacity(_shipCount + 1);
            _ships[_shipCount] = ship;
            _shipCount++;
            return _shipCount - 1;
        }

        /// <summary>Пытается получить корабль по слоту.</summary>
        public bool TryGetShip(int slot, out Ship ship)
        {
            if ((uint)slot < _shipCount)
            {
                ship = _ships[slot];
                return true;
            }

            ship = default;
            return false;
        }

        /// <summary>Обновляет корабль по слоту.</summary>
        public bool TryUpdateShip(int slot, in Ship ship)
        {
            if ((uint)slot >= _shipCount)
                return false;

            _ships[slot] = ship;
            return true;
        }

        /// <summary>Удаляет корабль из слота и возвращает его.</summary>
        public bool TryRemoveShip(int slot, out Ship ship)
        {
            if ((uint)slot >= _shipCount)
            {
                ship = default;
                return false;
            }

            ship = _ships[slot];
            var lastIndex = _shipCount - 1;
            if (slot != lastIndex)
                _ships[slot] = _ships[lastIndex];

            _ships[lastIndex] = default;
            _shipCount--;
            return true;
        }

        /// <summary>Полностью очищает состояние системы.</summary>
        public void Reset()
        {
            if (_shipCount > 0)
                Array.Clear(_ships, 0, _shipCount);

            _shipCount = 0;
        }

        /// <summary>Копирует активные корабли в внешний буфер и возвращает их количество.</summary>
        public int CopyShips(ref Ship[] buffer)
        {
            if (_shipCount <= 0)
                return 0;

            if (buffer == null || buffer.Length < _shipCount)
                buffer = new Ship[_shipCount];

            Array.Copy(_ships, 0, buffer, 0, _shipCount);
            return _shipCount;
        }

        /// <summary>Обеспечивает достаточную ёмкость буфера кораблей.</summary>
        private void EnsureCapacity(int needed)
        {
            if (_ships.Length >= needed)
                return;

            var newCapacity = _ships.Length == 0 ? 8 : _ships.Length * 2;
            while (newCapacity < needed)
                newCapacity *= 2;

            Array.Resize(ref _ships, newCapacity);
        }
    }
}
