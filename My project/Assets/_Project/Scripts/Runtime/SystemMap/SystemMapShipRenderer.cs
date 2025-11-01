using System.Collections.Generic;
using _Project.Prefabs;
using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using UnityEngine;

namespace _Project.Scripts.SystemMap
{
    [DisallowMultipleComponent]
    public sealed class SystemMapShipRenderer : MonoBehaviour, ISystemMapLayer
    {
        [Header("Порядок слоя")]
        [SerializeField] private int order = 10;     // чем больше число, тем позже рисуем
        public int Order => order;

        [Header("Каталог префабов")]
        [SerializeField] private PrefabCatalog catalog;

        private Transform _root;                     // общий родитель для всех кораблей на карте
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

        public void Render(in StarSys sys, Ship[] ships, int shipCount)
        {
            // ships и shipCount — свежий снимок кораблей от GameStateService
            var seen = HashSetPool<UID>.Get();

            if (ships != null && shipCount > 0)
            {
                for (int i = 0; i < shipCount; i++)
                {
                    var sh = ships[i];
                    seen.Add(sh.Uid);

                    if (!_views.TryGetValue(sh.Uid, out var view) || !view)
                    {
                        var prefab = GetShipPrefab();
                        if (!prefab)
                            continue;

                        view = Instantiate(prefab, _root);
                        view.name = $"Ship_{sh.Uid.Id}";
                        _views[sh.Uid] = view;
                    }

                    // пока обновляем только позицию — разворот добавим в следующих итерациях
                    view.transform.localPosition = sh.Position;
                }
            }

            if (_views.Count > 0)
            {
                ScratchKeys.Clear();
                ScratchKeys.AddRange(_views.Keys);

                for (int k = 0; k < ScratchKeys.Count; k++)
                {
                    var id = ScratchKeys[k];
                    if (!seen.Contains(id))
                    {
                        if (_views[id])
                            Destroy(_views[id]);
                        _views.Remove(id);
                    }
                }
            }

            HashSetPool<UID>.Release(seen);
        }

        public void Dispose() => ClearAll();

        private GameObject GetShipPrefab()
        {
            if (!catalog || catalog.ShipPrefabsByClass == null || catalog.ShipPrefabsByClass.Length == 0)
                return null;

            // временно берём первый класс корабля
            return catalog.ShipPrefabsByClass[0];
        }

        private void ClearAll()
        {
            foreach (var kv in _views)
            {
                if (kv.Value)
                    Destroy(kv.Value);
            }
            _views.Clear();

            if (_root)
            {
                for (int i = _root.childCount - 1; i >= 0; i--)
                    Destroy(_root.GetChild(i).gameObject);
            }
        }

        private static class HashSetPool<T>
        {
            private static readonly Stack<HashSet<T>> Pool = new();

            public static HashSet<T> Get() => Pool.Count > 0 ? Pool.Pop() : new HashSet<T>();

            public static void Release(HashSet<T> set)
            {
                set.Clear();
                Pool.Push(set);
            }
        }

        private static readonly List<UID> ScratchKeys = new();
    }
}
