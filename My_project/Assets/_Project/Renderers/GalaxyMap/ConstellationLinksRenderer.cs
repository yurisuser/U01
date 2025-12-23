using System.Collections.Generic;
using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Galaxy.Data;
using UnityEngine;

namespace _Project.Scripts.GalaxyMap.Runtime
{
    [DisallowMultipleComponent]
    /// <summary>Рисует гиперпереходы между звёздными системами на карте галактики.</summary>
    public sealed class ConstellationLinksRenderer : MonoBehaviour
    {
        [Header("Line settings")]
        [SerializeField] private Material lineMaterial;
        [SerializeField] private Color lineColor = new Color(0.4f, 0.8f, 1f, 0.5f);
        [SerializeField] private float lineWidthAtRefZoom = 0.08f;
        [SerializeField] private float referenceOrthoSize = 60f;

        [Header("Render root")]
        [SerializeField] private Transform linksRoot;

        [Header("Camera")]
        [SerializeField] private Camera targetCamera;

        private readonly List<LineRenderer> _lines = new();
        private GameStateService _state;
        private Material _runtimeMaterial;

        private void Awake()
        {
            if (!linksRoot)
            {
                var root = new GameObject("ConstellationLinksRoot");
                root.transform.SetParent(transform, false);
                linksRoot = root.transform;
            }

            EnsureMaterial();
        }

        private void OnEnable()
        {
            _state = GameBootstrap.GameState;
            if (_state != null)
            {
                _state.StateChanged += OnStateChanged;
                OnStateChanged();
            }
        }

        private void OnDisable()
        {
            if (_state != null)
                _state.StateChanged -= OnStateChanged;
            _state = null;

            if (_runtimeMaterial)
            {
                Destroy(_runtimeMaterial);
                _runtimeMaterial = null;
            }
        }

        private void OnStateChanged()
        {
            Render(_state?.Galaxy, clearBefore: true);
        }

        public void Render(StarSys[] systems, bool clearBefore = true)
        {
            if (clearBefore)
                ClearLines();

            if (systems == null || systems.Length == 0)
                return;

            EnsureMaterial();
            if (!lineMaterial)
                return;

            var parent = linksRoot ? linksRoot : transform;
            var edgeSet = new HashSet<long>();

            for (int i = 0; i < systems.Length; i++)
            {
                var links = systems[i].links;
                if (links == null || links.Length == 0)
                    continue;

                for (int j = 0; j < links.Length; j++)
                {
                    int other = links[j];
                    if (other < 0 || other >= systems.Length)
                        continue;

                    long key = GetEdgeKey(i, other);
                    if (!edgeSet.Add(key))
                        continue;

                    var line = CreateLine(parent);
                    line.SetPosition(0, systems[i].GalaxyPosition);
                    line.SetPosition(1, systems[other].GalaxyPosition);
                    _lines.Add(line);
                }
            }
        }

        private LineRenderer CreateLine(Transform parent)
        {
            var go = new GameObject("ConstellationLink");
            go.transform.SetParent(parent, false);

            var lr = go.AddComponent<LineRenderer>();
            lr.sharedMaterial = lineMaterial;
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.alignment = LineAlignment.View;
            lr.textureMode = LineTextureMode.Stretch;
            lr.widthMultiplier = lineWidthAtRefZoom;
            lr.startColor = lineColor;
            lr.endColor = lineColor;

            return lr;
        }

        // Гарантируем материал для линий даже без ручной настройки.
        private void EnsureMaterial()
        {
            if (lineMaterial)
                return;

            var shader = Shader.Find("Sprites/Default");
            if (!shader)
                return;

            _runtimeMaterial = new Material(shader)
            {
                name = "ConstellationLinksRuntimeMaterial"
            };
            _runtimeMaterial.hideFlags = HideFlags.DontSave;
            lineMaterial = _runtimeMaterial;
        }

        private void ClearLines()
        {
            for (int i = 0; i < _lines.Count; i++)
            {
                var lr = _lines[i];
                if (!lr)
                    continue;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(lr.gameObject);
                else
#endif
                    Destroy(lr.gameObject);
            }
            _lines.Clear();
        }

        private void LateUpdate()
        {
            if (_lines.Count == 0 || !targetCamera)
                return;

            float camOrtho = Mathf.Max(0.0001f, targetCamera.orthographicSize);
            float width = lineWidthAtRefZoom * (camOrtho / referenceOrthoSize);

            for (int i = 0; i < _lines.Count; i++)
            {
                var lr = _lines[i];
                if (!lr)
                    continue;

                lr.widthMultiplier = width;
            }
        }

        private static long GetEdgeKey(int a, int b)
        {
            int min = a < b ? a : b;
            int max = a < b ? b : a;
            return ((long)min << 32) | (uint)max;
        }
    }
}
