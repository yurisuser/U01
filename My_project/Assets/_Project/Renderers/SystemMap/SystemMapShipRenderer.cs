using System.Collections.Generic;
using UnityEngine;
using _Project.Prefabs;
using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState.GameStateMembers.SelectedObj;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using _Project.Scripts.Selection;

namespace _Project.Scripts.SystemMap
{
    /// <summary>Отрисовывает корабли выбранной системы с интерполяцией между снапшотами.</summary>
    public sealed class SystemMapShipRenderer : MonoBehaviour, ISystemMapLayer
    {
        [SerializeField] private int order = 5;
        [SerializeField] private PrefabCatalog catalog;
        [SerializeField] private float shipScale = 0.5f;

        private Transform _layerRoot;
        private readonly Dictionary<int, Transform> _shipInstances = new();
        private readonly List<int> _reusableKeys = new();
        private GameObject _resolvedShipPrefab;

        public int Order => order;

        public void Init(Transform parentRoot)
        {
            if (!_layerRoot)
            {
                var root = new GameObject("ShipsLayer");
                root.transform.SetParent(parentRoot, false);
                _layerRoot = root.transform;
            }

            ClearAll();
        }

        public void Render(in StarSys sys)
        {
            if (_layerRoot == null)
                return;

            var state = sys.State;
            if (state == null)
            {
                ClearAll();
                return;
            }

            var current = state.CurrShipSnapshots;
            var previous = state.PrevShipSnapshots;
            float lerpT = state.GetShipInterpolation(Time.unscaledTime);

            _reusableKeys.Clear();
            foreach (var key in _shipInstances.Keys)
                _reusableKeys.Add(key);

            for (int i = 0; i < current.Count; i++)
            {
                var ship = current[i];
                int key = ship.Uid.Id;
                var prefab = ResolveShipPrefab(in ship);
                if (!prefab)
                    continue;
                var view = GetOrCreateView(prefab, key, ship);
                _reusableKeys.Remove(key);

                UpdateTransform(view, ship, previous, lerpT);
            }

            for (int i = 0; i < _reusableKeys.Count; i++)
                RemoveView(_reusableKeys[i]);
        }

        public void Dispose() => ClearAll();

        private Transform GetOrCreateView(GameObject prefab, int key, in Ship ship)
        {
            if (_shipInstances.TryGetValue(key, out var view) && view)
            {
                var existingSelectable = view.GetComponent<SelectableData>();
                if (existingSelectable != null && !existingSelectable.HasData)
                    existingSelectable.SetData(GameBootstrap.GameState.SelectedSystemIndex, ship.Uid, ESelectedObjectType.Ship);
                return view;
            }

            var go = Instantiate(prefab, _layerRoot);
            go.name = $"Ship_{ship.Uid.Id}";
            var selectable = go.GetComponent<SelectableData>();
            if (selectable != null)
                selectable.SetData(GameBootstrap.GameState.SelectedSystemIndex, ship.Uid, ESelectedObjectType.Ship);
            var transform = go.transform;
            float prefabScale = ship.PrefabSize > 0f ? ship.PrefabSize : 1f;
            transform.localScale = Vector3.one * Mathf.Max(0.0001f, shipScale * prefabScale);
            _shipInstances[key] = transform;
            return transform;
        }

        private void UpdateTransform(Transform target, in Ship current, IReadOnlyList<Ship> prev, float lerpT)
        {
            if (!target)
                return;

            if (!TryFindShip(prev, current.Uid.Id, out var prevShip))
            {
                prevShip = current;
                lerpT = 1f;
            }

            var position = Vector3.Lerp(prevShip.Position, current.Position, lerpT);
            var rotation = Quaternion.Slerp(prevShip.Rotation, current.Rotation, lerpT);

            target.localPosition = position;
            target.localRotation = rotation;
        }

        private void RemoveView(int key)
        {
            if (_shipInstances.TryGetValue(key, out var view))
            {
                if (view)
                    Destroy(view.gameObject);
                _shipInstances.Remove(key);
            }
        }

        private void ClearAll()
        {
            foreach (var view in _shipInstances.Values)
            {
                if (view)
                    Destroy(view.gameObject);
            }

            _shipInstances.Clear();
            _reusableKeys.Clear();
        }

        private GameObject ResolveShipPrefab(in Ship ship)
        {
            if (catalog != null && catalog.ShipPrefabsByKey != null && !string.IsNullOrWhiteSpace(ship.PrefabKey))
            {
                var entries = catalog.ShipPrefabsByKey;
                for (int i = 0; i < entries.Length; i++)
                {
                    if (string.Equals(entries[i].Key, ship.PrefabKey, System.StringComparison.OrdinalIgnoreCase))
                        return entries[i].Prefab;
                }
            }

            if (_resolvedShipPrefab)
                return _resolvedShipPrefab;

            if (catalog == null || catalog.ShipPrefabsByClass == null || catalog.ShipPrefabsByClass.Length == 0)
            {
                UnityEngine.Debug.LogWarning("[SystemMap][Ships] Не задан PrefabCatalog или массив ShipPrefabsByClass пуст.", this);
                return null;
            }

            var prefab = catalog.ShipPrefabsByClass[0];
            if (!prefab)
            {
                UnityEngine.Debug.LogWarning("[SystemMap][Ships] Ship prefab по индексу 0 не задан в каталоге.", this);
                return null;
            }

            _resolvedShipPrefab = prefab;
            return _resolvedShipPrefab;
        }

        private static bool TryFindShip(IReadOnlyList<Ship> list, int uid, out Ship ship)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var candidate = list[i];
                if (candidate.Uid.Id == uid)
                {
                    ship = candidate;
                    return true;
                }
            }

            ship = default;
            return false;
        }
    }
}
