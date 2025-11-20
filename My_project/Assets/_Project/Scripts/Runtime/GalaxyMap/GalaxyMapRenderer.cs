using System.Collections.Generic;
using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Galaxy.Data;
using _Project.Prefabs; // prefab catalog access
using UnityEngine;

namespace _Project.Scripts.GalaxyMap.Runtime
{
    [DisallowMultipleComponent]
    /// <summary>Отрисовывает объекты галактической карты по данным снапшота.</summary>
    public class GalaxyMapRenderer : MonoBehaviour
    {
        [Header("Prefab catalogue")]
        [SerializeField] private PrefabCatalog catalog; // maps star types to prefabs

        [Header("Fallback prefab")]
        [SerializeField] private GameObject defaultPrefab;

        [Header("Star size multipliers (default = 1)")]
         private float dwarfMul = 0.7f;
         private float normalMul = 1.0f;
        private float giantMul = 1.4f;
         private float supergiantMul = 2.0f;
        private float globalScale = 2.5f;

        [Header("Spawn root transform")]
        [SerializeField] private Transform starsRoot;

        private readonly List<GameObject> _spawned = new();
        private GameStateService _state;

        public IReadOnlyList<GameObject> Spawned => _spawned;

        private void Awake()
        {
            if (!starsRoot)
            {
                var root = new GameObject("StarsRoot");
                root.transform.SetParent(transform, false);
                starsRoot = root.transform;
            }
        }

        private void OnEnable()
        {
            _state = GameBootstrap.GameState;
            if (_state != null)
            {
                _state.RenderChanged += OnRenderChanged;
                OnRenderChanged(_state.Render); // update immediately to keep map in sync
            }
        }

        private void OnDisable()
        {
            if (_state != null)
                _state.RenderChanged -= OnRenderChanged;
            _state = null;
        }

        private void OnRenderChanged(GameStateService.RenderSnapshot snapshot)
        {
            Render(snapshot.Galaxy, clearBefore: true);
        }

        public void Render(StarSys[] systems, bool clearBefore = true)
        {
            if (clearBefore)
                ClearSpawned();

            if (systems == null || systems.Length == 0)
                return;

            var parent = starsRoot ? starsRoot : transform;

            for (int i = 0; i < systems.Length; i++)
            {
                var s = systems[i];

                var prefab = GetPrefabFor(s.Star.type) ?? defaultPrefab; // prefer type-specific prefab
                if (!prefab)
                    continue;

                var go = Instantiate(prefab, s.GalaxyPosition, Quaternion.identity, parent);
                go.name = string.IsNullOrWhiteSpace(s.Name) ? $"SYS-{i:0000}" : s.Name;

                // scale the visual based on star size and global multiplier
                var mul = GetSizeMul(s.Star.size) * Mathf.Max(0.0001f, globalScale);
                go.transform.localScale = go.transform.localScale * mul;

                // configure click handler with metadata if present
                var click = go.GetComponent<StarGalaxyMapClick>();
                if (click != null)
                {
                    click.type = s.Star.type;
                    click.systemName = go.name;
                    click.System = s;
                }

                _spawned.Add(go);
            }
        }

        private void ClearSpawned()
        {
            for (int i = 0; i < _spawned.Count; i++)
            {
                var go = _spawned[i];
                if (!go)
                    continue;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(go);
                else
#endif
                    Destroy(go);
            }
            _spawned.Clear();
        }

        // === Prefab catalog helpers ===
        private GameObject GetPrefabFor(EStarType t)
        {
            if (!catalog || catalog.StarGalaxyPrefabsByType == null)
                return null;

            var arr = catalog.StarGalaxyPrefabsByType;
            var index = (int)t;
            if (index < 0 || index >= arr.Length)
                return null;

            return arr[index];
        }
        // ==============================

        private float GetSizeMul(EStarSize z) =>
            z switch
            {
                EStarSize.Dwarf => dwarfMul,
                EStarSize.Normal => normalMul,
                EStarSize.Giant => giantMul,
                EStarSize.Supergiant => supergiantMul,
                _ => normalMul
            };
    }
}
