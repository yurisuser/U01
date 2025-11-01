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
        [SerializeField] private int order = 10;
        public int Order => order;

        [Header("Каталог префабов")]
        [SerializeField] private PrefabCatalog catalog;

        private Transform _root;
        private readonly Dictionary<UID, GameObject> _views = new();
        private readonly Dictionary<UID, Ship> _prevShips = new();

        public void Init(Transform parentRoot)
        {
            if (_root == null)
            {
                var go = new GameObject("ShipsRoot");
                go.transform.SetParent(parentRoot, false);
                _root = go.transform;
            }
        }

        public void Render(in StarSys sys,
                            Ship[] prevShips,
                            int prevCount,
                            Ship[] currShips,
                            int currCount,
                            float progress)
        {
            progress = Mathf.Clamp01(progress);

            _prevShips.Clear();
            if (prevShips != null && prevCount > 0)
            {
                for (int i = 0; i < prevCount; i++)
                    _prevShips[prevShips[i].Uid] = prevShips[i];
            }

            var seen = HashSetPool<UID>.Get();

            if (currShips != null && currCount > 0)
            {
                for (int i = 0; i < currCount; i++)
                {
                    var sh = currShips[i];
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

                    Vector3 startPos = sh.Position;
                    Quaternion startRot = sh.Rotation;
                    if (_prevShips.TryGetValue(sh.Uid, out var prev))
                    {
                        startPos = prev.Position;
                        startRot = prev.Rotation;
                    }

                    var pos = Vector3.Lerp(startPos, sh.Position, progress);
                    var rot = Quaternion.Slerp(startRot, sh.Rotation, progress);

                    view.transform.localPosition = pos;
                    view.transform.localRotation = rot;
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
            _prevShips.Clear();
        }

        public void Dispose() => ClearAll();

        private GameObject GetShipPrefab()
        {
            if (!catalog || catalog.ShipPrefabsByClass == null || catalog.ShipPrefabsByClass.Length == 0)
                return null;

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

            _prevShips.Clear();
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

