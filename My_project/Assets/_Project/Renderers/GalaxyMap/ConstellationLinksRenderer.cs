using System.Collections.Generic;
using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.NPC.Fraction;
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
        [SerializeField] private float constellationSaturation = 0.75f;
        [SerializeField] private float constellationValue = 0.9f;
        [SerializeField] private float constellationAlpha = 0.65f;
        [SerializeField] private Color emptyConstellationColor = new Color(0.6f, 0.85f, 1f, 0.65f);
        [SerializeField] private Color noColoringLinkColor = new Color(1f, 1f, 1f, 0.85f);
        [SerializeField] private float lineWidthAtRefZoom = 0.08f;
        [SerializeField] private float referenceOrthoSize = 60f;
        [SerializeField] private Color interLinkColor = new Color(1f, 1f, 1f, 0.85f);
        [SerializeField] private float interLinkWidthMultiplier = 2f;
        [SerializeField] private int interLinkDotCount = 10;

        [Header("Render root")]
        [SerializeField] private Transform linksRoot;

        [Header("Camera")]
        [SerializeField] private Camera targetCamera;

        private readonly List<LineRenderer> _lines = new();
        private readonly List<HyperlinkEdge> _lineEdges = new();
        private readonly List<float> _lineWidthScales = new();
        private GameStateService _state;
        private Material _runtimeMaterial;
        private StarSys[] _renderedGalaxy;
        private bool _useHyperlinkColoring;
        private bool _useFractionColoring;
        private Color[] _constellationColors;

        internal float ConstellationSaturation => constellationSaturation;
        internal float ConstellationValue => constellationValue;
        internal float ConstellationAlpha => constellationAlpha;
        internal Color EmptyConstellationColor => emptyConstellationColor;
        internal Color NoColoringLinkColor => noColoringLinkColor;
        internal Color LineColor => lineColor;
        internal Color[] ConstellationColors => _constellationColors;

        internal Color[] GetConstellationColors()
        {
            if (_constellationColors == null && _renderedGalaxy != null)
                BuildConstellationColors();
            return _constellationColors;
        }

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
            if (_state != null && !_state.ShowHyperlinks)
            {
                SetLinesVisible(false);
                _useHyperlinkColoring = _state.UseHyperlinkColoring;
                _useFractionColoring = _state.UseFractionColoring;
                return;
            }

            if (_state == null)
                return;

            SetLinesVisible(true);

            bool newColoring = _state.UseHyperlinkColoring;
            bool newFractions = _state.UseFractionColoring;
            if (_renderedGalaxy != _state.Galaxy)
            {
                _useHyperlinkColoring = newColoring;
                _useFractionColoring = newFractions;
                Render(_state.Galaxy, clearBefore: true);
                return;
            }

            if (_useHyperlinkColoring != newColoring || _useFractionColoring != newFractions)
            {
                _useHyperlinkColoring = newColoring;
                if (_useFractionColoring != newFractions)
                    _constellationColors = null;
                _useFractionColoring = newFractions;
                ApplyLineColors();
            }
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

            _renderedGalaxy = systems;
            _useHyperlinkColoring = _state != null && _state.UseHyperlinkColoring;
            _useFractionColoring = _state != null && _state.UseFractionColoring;
            BuildConstellationColors();

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

                    bool isInter = systems[i].ConstellationId != systems[other].ConstellationId;
                    CreateLinkVisual(parent, systems[i].GalaxyPosition, systems[other].GalaxyPosition, i, other, isInter);
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
            _lineEdges.Clear();
            _lineWidthScales.Clear();
            _renderedGalaxy = null;
            _constellationColors = null;
        }

        private void SetLinesVisible(bool visible)
        {
            for (int i = 0; i < _lines.Count; i++)
            {
                var lr = _lines[i];
                if (!lr)
                    continue;

                lr.enabled = visible;
            }
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

                float scale = i < _lineWidthScales.Count ? _lineWidthScales[i] : 1f;
                lr.widthMultiplier = width * scale;
            }
        }

        private void ApplyLineColors()
        {
            if (_renderedGalaxy == null || _lines.Count != _lineEdges.Count)
                return;

            if (_useHyperlinkColoring && _constellationColors == null)
                BuildConstellationColors();

            for (int i = 0; i < _lines.Count; i++)
            {
                var lr = _lines[i];
                if (!lr)
                    continue;

                var edge = _lineEdges[i];
                bool isInter = _renderedGalaxy[edge.A].ConstellationId != _renderedGalaxy[edge.B].ConstellationId;
                ApplyLineColor(lr, edge.A, edge.B, isInter);
            }
        }

        private void ApplyLineColor(LineRenderer line, int a, int b, bool isInter)
        {
            if (line == null)
                return;

            if (isInter)
            {
                line.startColor = interLinkColor;
                line.endColor = interLinkColor;
                return;
            }

            if (_renderedGalaxy == null || a < 0 || b < 0 || a >= _renderedGalaxy.Length || b >= _renderedGalaxy.Length)
            {
                line.startColor = lineColor;
                line.endColor = lineColor;
                return;
            }

            if (!_useHyperlinkColoring && !_useFractionColoring)
            {
                var flat = noColoringLinkColor;
                line.startColor = flat;
                line.endColor = flat;
                return;
            }

            if (_useFractionColoring && TryGetFractionLinkColors(a, b, out var fracA, out var fracB))
            {
                line.startColor = fracA;
                line.endColor = fracB;
                return;
            }

            if (_useHyperlinkColoring)
            {
                var colorA = GetConstellationColor(_renderedGalaxy[a].ConstellationId);
                var colorB = GetConstellationColor(_renderedGalaxy[b].ConstellationId);
                line.startColor = colorA;
                line.endColor = colorB;
                return;
            }

            var fallback = noColoringLinkColor;
            line.startColor = fallback;
            line.endColor = fallback;
        }

        private void CreateLinkVisual(Transform parent, Vector3 aPos, Vector3 bPos, int aIndex, int bIndex, bool isInter)
        {
            if (!isInter)
            {
                var line = CreateLine(parent);
                line.SetPosition(0, aPos);
                line.SetPosition(1, bPos);
                _lines.Add(line);
                _lineEdges.Add(new HyperlinkEdge(aIndex, bIndex));
                _lineWidthScales.Add(1f);
                    ApplyLineColor(line, aIndex, bIndex, false);
                return;
            }

            Vector3 dir = (bPos - aPos);
            float dist = dir.magnitude;
            if (dist <= 0.0001f)
                return;

            dir /= dist;
            int dotCount = Mathf.Max(1, interLinkDotCount);
            int gapCount = Mathf.Max(0, dotCount - 1);
            float unit = dist / (dotCount + gapCount);
            if (unit <= 0.0001f)
                return;

            float length = unit;
            float gap = gapCount > 0 ? unit : 0f;
            float spacing = length + gap;

            for (int i = 0; i < dotCount; i++)
            {
                Vector3 start = aPos + dir * (i * spacing);
                Vector3 end = start + dir * length;
                if (Vector3.Dot(end - bPos, dir) > 0f)
                    end = bPos;

                var line = CreateLine(parent);
                line.SetPosition(0, start);
                line.SetPosition(1, end);
                _lines.Add(line);
                _lineEdges.Add(new HyperlinkEdge(aIndex, bIndex));
                _lineWidthScales.Add(interLinkWidthMultiplier);
                ApplyLineColor(line, aIndex, bIndex, true);
            }
        }

        private static long GetEdgeKey(int a, int b)
        {
            int min = a < b ? a : b;
            int max = a < b ? b : a;
            return ((long)min << 32) | (uint)max;
        }

        // Цвет по id созвездия: стабильный, но визуально различимый.
        private Color GetConstellationColor(int constellationId)
        {
            if (constellationId <= 0)
            {
                var empty = emptyConstellationColor;
                empty.a = constellationAlpha;
                return empty;
            }

            if (_constellationColors == null || constellationId >= _constellationColors.Length)
            {
                float hue = Hash01(constellationId);
                var fallback = Color.HSVToRGB(hue, constellationSaturation, constellationValue);
                fallback.a = constellationAlpha;
                return fallback;
            }

            return _constellationColors[constellationId];
        }

        private static float Hash01(int seed)
        {
            unchecked
            {
                uint x = (uint)seed;
                x ^= x >> 17;
                x *= 0xED5AD4BBu;
                x ^= x >> 11;
                x *= 0xAC4C1B51u;
                x ^= x >> 15;
                x *= 0x31848BABu;
                x ^= x >> 14;
                return (x & 0xFFFFFFu) / 16777216f;
            }
        }

        private void BuildConstellationColors()
        {
            _constellationColors = GalaxyMapColorProvider.BuildConstellationColors(
                _renderedGalaxy,
                _useFractionColoring,
                constellationSaturation,
                constellationValue,
                constellationAlpha,
                emptyConstellationColor);
        }

        private static bool TryGetFractionColor(in Fraction fraction, out Color color)
        {
            color = default;
            if (string.IsNullOrWhiteSpace(fraction.Color))
                return false;

            if (!ColorUtility.TryParseHtmlString(fraction.Color, out var parsed))
                return false;

            color = parsed;
            return true;
        }

        private bool TryGetFractionLinkColors(int a, int b, out Color colorA, out Color colorB)
        {
            colorA = default;
            colorB = default;

            bool hasA = TryGetSystemFractionColor(a, out var aColor);
            bool hasB = TryGetSystemFractionColor(b, out var bColor);

            if (!hasA && !hasB)
                return false;

            colorA = hasA ? aColor : bColor;
            colorB = hasB ? bColor : aColor;
            return true;
        }

        private bool TryGetSystemFractionColor(int systemIndex, out Color color)
        {
            color = default;
            if (!_useFractionColoring || _renderedGalaxy == null)
                return false;

            if (systemIndex < 0 || systemIndex >= _renderedGalaxy.Length)
                return false;

            var owner = _renderedGalaxy[systemIndex].OwnerFrac;
            if (owner.Id <= 0)
                return false;

            if (!TryGetFractionColor(owner, out var parsed))
                return false;

            parsed.a = constellationAlpha;
            color = parsed;
            return true;
        }
    }
}
