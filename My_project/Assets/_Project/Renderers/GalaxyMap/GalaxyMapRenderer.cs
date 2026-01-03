using System.Collections.Generic;
using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Galaxy.Data;
using _Project.Prefabs; // prefab catalog access
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

        [Header("Debug gizmos")]
        [SerializeField] private bool drawConstellationGizmos = true;
        [SerializeField] private Color rowsColor = new Color(1f, 1f, 1f, 0.15f);
        [SerializeField] private Color segmentsColor = new Color(0.2f, 1f, 0.2f, 0.2f);
        [SerializeField] private Color coreColor = new Color(1f, 0.2f, 0.2f, 0.2f);
        [SerializeField] private bool drawConstellationIds = true;
        [SerializeField] private Color constellationIdColor = new Color(1f, 0.9f, 0.4f, 0.9f);
        [SerializeField] private int constellationIdFontSize = 12;

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
                _state.StateChanged += OnStateChanged;
                OnStateChanged(); // update immediately to keep map in sync
            }
        }

        private void OnDisable()
        {
            if (_state != null)
                _state.StateChanged -= OnStateChanged;
            _state = null;
        }

        private void OnDrawGizmos()
        {
            if (!drawConstellationGizmos)
                return;

            var rows = _Project.Scripts.Galaxy.Generation.ConstellationCreator.GetSectorsRows();
            var segments = _Project.Scripts.Galaxy.Generation.ConstellationCreator.GetSectorRowsSegments();
            if (rows == null || segments == null || rows.Length == 0 || segments.Length == 0)
                return;

            float innerRadius = _Project.Scripts.Const.GalaxyConstants.MinStarInterval
                                * _Project.Scripts.Const.GalaxyConstants.CentralBlackHoleIntervalK;

            Vector3 center = transform.position;
            DrawCircle(center, innerRadius, coreColor);

            for (int i = 0; i < rows.Length; i++)
            {
                float outerRadius = rows[i];
                DrawCircle(center, outerRadius, rowsColor);

                float startRadius = (i == 0) ? innerRadius : rows[i - 1];
                if (i >= segments.Length || segments[i] == null || segments[i].Length == 0)
                    continue;

                var angles = segments[i];
                for (int s = 0; s < angles.Length; s++)
                {
                    float a = angles[s];
                    var from = center + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * startRadius;
                    var to = center + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * outerRadius;
                    Gizmos.color = segmentsColor;
                    Gizmos.DrawLine(from, to);
                }
            }

#if UNITY_EDITOR
            if (drawConstellationIds)
                DrawConstellationIds(center, innerRadius, rows, segments);
#endif
        }

        private void OnStateChanged()
        {
            Render(_state?.Galaxy, clearBefore: true);
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

        private static void DrawCircle(Vector3 center, float radius, Color color)
        {
            if (radius <= 0f)
                return;

            Gizmos.color = color;
            const int steps = 128;
            float step = Mathf.PI * 2f / steps;
            Vector3 prev = center + new Vector3(radius, 0f, 0f);
            for (int i = 1; i <= steps; i++)
            {
                float a = step * i;
                Vector3 next = center + new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }

#if UNITY_EDITOR
        private void DrawConstellationIds(Vector3 center, float innerRadius, int[] rows, float[][] segments)
        {
            if (rows == null || segments == null || rows.Length == 0 || segments.Length == 0)
                return;

            var sectorsPerRow = _Project.Scripts.Const.GalaxyConstants.ConstellationSectors;
            if (sectorsPerRow == null || sectorsPerRow.Length == 0)
                return;

            int rowsCount = Mathf.Min(rows.Length, segments.Length);
            rowsCount = Mathf.Min(rowsCount, sectorsPerRow.Length);
            if (rowsCount <= 0)
                return;

            var style = new GUIStyle
            {
                fontSize = Mathf.Max(1, constellationIdFontSize),
                normal = new GUIStyleState { textColor = constellationIdColor }
            };

            int idBase = 1;
            const float fullCircle = Mathf.PI * 2f;

            for (int r = 0; r < rowsCount; r++)
            {
                float outerRadius = rows[r];
                float startRadius = (r == 0) ? innerRadius : rows[r - 1];
                float midRadius = (startRadius + outerRadius) * 0.5f;

                var angles = segments[r];
                if (angles == null || angles.Length == 0)
                    continue;

                int segCount = Mathf.Max(1, sectorsPerRow[r]);
                for (int s = 0; s < segCount; s++)
                {
                    float a0 = angles[s % angles.Length];
                    float a1 = angles[(s + 1) % angles.Length];
                    if (a1 <= a0)
                        a1 += fullCircle;
                    float midAngle = (a0 + a1) * 0.5f;
                    if (midAngle >= fullCircle)
                        midAngle -= fullCircle;

                    int cid = idBase + s;
                    var pos = center + new Vector3(Mathf.Cos(midAngle), Mathf.Sin(midAngle), 0f) * midRadius;
                    Handles.Label(pos, cid.ToString(), style);
                }

                idBase += segCount;
            }
        }
#endif
    }
}
