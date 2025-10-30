using System.Collections.Generic;
using _Project.Prefabs;
using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using UnityEngine;
using _Project.Scripts.SystemMap;

namespace _Project.Scripts.SystemMap
{
    [DisallowMultipleComponent]
    public sealed class SystemMapShipRenderer : MonoBehaviour, ISystemMapLayer
    {
        [Header("Порядок слоя")]
        [SerializeField] private int order = 10;     // позже кораблей можно рисовать поверх гео
        public int Order => order;

        [Header("Каталог префабов")]
        [SerializeField] private PrefabCatalog catalog;

        private Transform _root;                     // ShipsRoot
        private readonly Dictionary<UID, GameObject> _views = new();

        public void Init(Transform parentRoot)
        {
            if (_root == null)
            {
                var go = new GameObject("ShipsRoot");
                go.transform.SetParent(parentRoot, false);
                _root = go.transform;
            }
            ClearAll();
        }

        public void Render(in StarSys sys)
        {
            // если кораблей нет — просто подчистим "лишние" из словаря
            var ships = sys.ShipArr;
            var seen = HashSetPool<UID>.Get();       // маленький пул локального набора id

            if (ships != null)
            {
                for (int i = 0; i < ships.Length; i++)
                {
                    var sh = ships[i];
                    seen.Add(sh.Uid);

                    // 1) Добавление
                    if (!_views.TryGetValue(sh.Uid, out var go) || !go)
                    {
                        var prefab = GetShipPrefab(); // пока один тип
                        if (!prefab) continue;
                        go = Instantiate(prefab, _root);
                        go.name = $"Ship_{sh.Uid.Id}";
                        _views[sh.Uid] = go;
                    }

                    // 2) Обновление позиции (ориентацию добавим позже)
                    go.transform.localPosition = sh.Position;
                }
            }

            // 3) Удаление пропавших
            if (_views.Count > 0)
            {
                ScratchKeys.Clear();
                ScratchKeys.AddRange(_views.Keys);
                for (int k = 0; k < ScratchKeys.Count; k++)
                {
                    var id = ScratchKeys[k];
                    if (!seen.Contains(id))
                    {
                        if (_views[id]) Destroy(_views[id]);
                        _views.Remove(id);
                    }
                }
            }

            HashSetPool<UID>.Release(seen);
        }

        public void Dispose() => ClearAll();

        // --- helpers ---

        private GameObject GetShipPrefab()
        {
            // Минимально: берём первый элемент Ship Prefabs By Class из каталога.
            if (!catalog || catalog.ShipPrefabsByClass == null || catalog.ShipPrefabsByClass.Length == 0)
                return null;
            return catalog.ShipPrefabsByClass[0];
        }

        private void ClearAll()
        {
            foreach (var kv in _views)
                if (kv.Value) Destroy(kv.Value);
            _views.Clear();
            if (_root)
            {
                for (int i = _root.childCount - 1; i >= 0; i--)
                    Destroy(_root.GetChild(i).gameObject);
            }
        }

        // Небольшой локальный пул для HashSet, чтобы не мусорить GC
        static class HashSetPool<T>
        {
            static readonly Stack<HashSet<T>> Pool = new();
            public static HashSet<T> Get() => Pool.Count > 0 ? Pool.Pop() : new HashSet<T>();
            public static void Release(HashSet<T> set) { set.Clear(); Pool.Push(set); }
        }

        // временный буфер ключей на удаление
        private static readonly List<UID> ScratchKeys = new();
    }
}
