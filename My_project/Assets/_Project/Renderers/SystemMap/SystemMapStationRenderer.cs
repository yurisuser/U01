using System.Collections.Generic;
using UnityEngine;
using _Project.Prefabs;
using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState.GameStateMembers.SelectedObj;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Selection;
using _Project.Scripts.Const;
using _Project.Scripts.Stations;

namespace _Project.Scripts.SystemMap
{
    /// <summary>Отрисовывает станции выбранной системы.</summary>
    public sealed class SystemMapStationRenderer : MonoBehaviour, ISystemMapLayer
    {
        [SerializeField] private int order = 2;
        [SerializeField] private PrefabCatalog catalog;
        [SerializeField] private float stationScale = 1f;

        private Transform _layerRoot;
        private readonly Dictionary<int, Transform> _instances = new();
        private readonly List<int> _reusableKeys = new();
        private GameObject _fallbackPrefab;

        public int Order => order;

        public void Init(Transform parentRoot)
        {
            if (!_layerRoot)
            {
                var root = new GameObject("StationsLayer");
                root.transform.SetParent(parentRoot, false);
                _layerRoot = root.transform;
            }

            ClearAll();
        }

        public void Render(in StarSys sys)
        {
            if (_layerRoot == null)
                return;

            var stations = sys.Stations;
            if (stations == null || stations.Length == 0)
            {
                ClearAll();
                return;
            }

            _reusableKeys.Clear();
            foreach (var key in _instances.Keys)
                _reusableKeys.Add(key);

            for (int i = 0; i < stations.Length; i++)
            {
                var station = stations[i];
                int key = station.Uid.Id;
                var prefab = ResolvePrefab(station.PrefabKey);
                if (!prefab)
                    continue;

                var view = GetOrCreateView(prefab, key, station);
                _reusableKeys.Remove(key);
                view.localPosition = station.Position;
            }

            for (int i = 0; i < _reusableKeys.Count; i++)
                RemoveView(_reusableKeys[i]);
        }

        public void Dispose() => ClearAll();

        private Transform GetOrCreateView(GameObject prefab, int key, in Station station)
        {
            if (_instances.TryGetValue(key, out var existing) && existing)
                return existing;

            var go = Instantiate(prefab, _layerRoot);
            go.name = string.IsNullOrWhiteSpace(station.TypeKey)
                ? $"Station_{station.Uid.Id}"
                : station.TypeKey;

            var selectable = go.GetComponent<SelectableData>();
            if (selectable != null)
                selectable.SetData(GameBootstrap.GameState.SelectedSystemIndex, station.Uid, ESelectedObjectType.Station);

            var t = go.transform;
            t.localScale = t.localScale * Mathf.Max(0.0001f, StarSysemConstants.StationPrefabScale * stationScale);
            _instances[key] = t;
            return t;
        }

        private void RemoveView(int key)
        {
            if (_instances.TryGetValue(key, out var view))
            {
                if (view)
                    Destroy(view.gameObject);
                _instances.Remove(key);
            }
        }

        private void ClearAll()
        {
            foreach (var view in _instances.Values)
            {
                if (view)
                    Destroy(view.gameObject);
            }

            _instances.Clear();
            _reusableKeys.Clear();
        }

        private GameObject ResolvePrefab(string prefabKey)
        {
            if (catalog != null && catalog.StationPrefabsByKey != null && !string.IsNullOrWhiteSpace(prefabKey))
            {
                var entries = catalog.StationPrefabsByKey;
                for (int i = 0; i < entries.Length; i++)
                {
                    if (string.Equals(entries[i].Key, prefabKey, System.StringComparison.OrdinalIgnoreCase))
                        return entries[i].Prefab;
                }
            }

            if (_fallbackPrefab)
                return _fallbackPrefab;

            if (catalog != null && catalog.StationPrefabsByKey != null && catalog.StationPrefabsByKey.Length > 0)
            {
                var candidate = catalog.StationPrefabsByKey[0].Prefab;
                if (candidate)
                {
                    _fallbackPrefab = candidate;
                    return _fallbackPrefab;
                }
            }

            UnityEngine.Debug.LogWarning("[SystemMap][Stations] Не найден префаб станции (catalog/StationPrefabsByKey).", this);
            return null;
        }
    }
}
