using System;
using System.Collections.Generic;
using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;

namespace _Project.Scripts.Core.Runtime
{
    /// <summary>
    /// Хранит статичное описание галактики и индексы для быстрого доступа.
    /// </summary>
    public sealed class GalaxyService
    {
        private StarSys[] _systems = Array.Empty<StarSys>();
        private readonly Dictionary<UID, int> _indexByUid = new Dictionary<UID, int>(128);

        public int Count => _systems.Length;

        public void Initialize(StarSys[] generated)
        {
            if (generated == null)
                throw new ArgumentNullException(nameof(generated));

            _systems = generated;
            _indexByUid.Clear();

            for (var i = 0; i < _systems.Length; i++)
            {
                var uid = _systems[i].Uid;
                if (!_indexByUid.ContainsKey(uid))
                    _indexByUid.Add(uid, i);
            }
        }

        public void Reset()
        {
            _systems = Array.Empty<StarSys>();
            _indexByUid.Clear();
        }

        public StarSys[] GetAll() => _systems;

        public bool TryGetIndex(UID uid, out int index)
        {
            return _indexByUid.TryGetValue(uid, out index);
        }

        public ref readonly StarSys GetByIndex(int index)
        {
            return ref _systems[index];
        }
    }
}
