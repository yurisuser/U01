using System;
using System.Collections.Generic;
using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;

namespace _Project.Scripts.Core.Runtime
{
    // Хранит статичное описание галактики и индексы для быстрого доступа.
    public sealed class GalaxyService
    {
        private StarSys[] _systems = Array.Empty<StarSys>(); // Список систем.
        private readonly Dictionary<UID, int> _indexByUid = new Dictionary<UID, int>(128); // Быстрый поиск по UID.

        public int Count => _systems.Length; // Количество систем.

        // Инициализируем сервис сгенерированным массивом.
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

        // Полностью очищаем сервис.
        public void Reset()
        {
            _systems = Array.Empty<StarSys>();
            _indexByUid.Clear();
        }

        public StarSys[] GetAll() => _systems; // Возвращаем все системы.

        // Пытаемся найти индекс системы по UID.
        public bool TryGetIndex(UID uid, out int index)
        {
            return _indexByUid.TryGetValue(uid, out index);
        }

        // Получаем систему по индексу.
        public ref readonly StarSys GetByIndex(int index)
        {
            return ref _systems[index];
        }
    }
}
