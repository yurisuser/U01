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
        [SerializeField] private float constellationSaturation = 0.75f;
        [SerializeField] private float constellationValue = 0.9f;
        [SerializeField] private float constellationAlpha = 0.65f;
        [SerializeField] private float lineWidthAtRefZoom = 0.08f;
        [SerializeField] private float referenceOrthoSize = 60f;
        [SerializeField] private float interLinkWidthMultiplier = 2f;
        [SerializeField] private float interLinkDotRatio = 0.3f;
        [SerializeField] private float interLinkGapRatio = 0.3f;

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
        private Color[] _constellationColors;

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
                return;
            }

            if (_state == null)
                return;

            SetLinesVisible(true);

            bool newColoring = _state.UseHyperlinkColoring;
            if (_renderedGalaxy != _state.Galaxy)
            {
                _useHyperlinkColoring = newColoring;
                Render(_state.Galaxy, clearBefore: true);
                return;
            }

            if (_useHyperlinkColoring != newColoring)
            {
                _useHyperlinkColoring = newColoring;
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
                ApplyLineColor(lr, edge.A, edge.B);
            }
        }

        private void ApplyLineColor(LineRenderer line, int a, int b)
        {
            if (line == null)
                return;

            if (_renderedGalaxy == null || a < 0 || b < 0 || a >= _renderedGalaxy.Length || b >= _renderedGalaxy.Length)
            {
                line.startColor = lineColor;
                line.endColor = lineColor;
                return;
            }

            if (!_useHyperlinkColoring)
            {
                var white = new Color(1f, 1f, 1f, constellationAlpha);
                line.startColor = white;
                line.endColor = white;
                return;
            }

            var colorA = GetConstellationColor(_renderedGalaxy[a].ConstellationId);
            var colorB = GetConstellationColor(_renderedGalaxy[b].ConstellationId);
            line.startColor = colorA;
            line.endColor = colorB;
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
                ApplyLineColor(line, aIndex, bIndex);
                return;
            }

            Vector3 dir = (bPos - aPos);
            float dist = dir.magnitude;
            if (dist <= 0.0001f)
                return;

            dir /= dist;
            float dotRatio = Mathf.Clamp01(interLinkDotRatio);
            float gapRatio = Mathf.Clamp01(interLinkGapRatio);
            float length = dist * dotRatio;
            float gap = dist * gapRatio;
            float spacing = Mathf.Max(0.05f, length + gap);
            length = Mathf.Clamp(length, 0.05f, spacing);
            int dotCount = Mathf.Max(1, Mathf.FloorToInt(dist / spacing));

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
                ApplyLineColor(line, aIndex, bIndex);
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
                return lineColor;

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
            if (_renderedGalaxy == null || _renderedGalaxy.Length == 0)
            {
                _constellationColors = null;
                return;
            }

            int maxId = 0;
            for (int i = 0; i < _renderedGalaxy.Length; i++)
            {
                int cid = _renderedGalaxy[i].ConstellationId;
                if (cid > maxId)
                    maxId = cid;
            }

            if (maxId <= 0)
            {
                _constellationColors = null;
                return;
            }

            var neighbors = new HashSet<int>[maxId + 1];
            for (int i = 0; i <= maxId; i++)
                neighbors[i] = new HashSet<int>();

            for (int i = 0; i < _renderedGalaxy.Length; i++)
            {
                int cidA = _renderedGalaxy[i].ConstellationId;
                if (cidA <= 0)
                    continue;

                var links = _renderedGalaxy[i].links;
                if (links == null)
                    continue;

                for (int j = 0; j < links.Length; j++)
                {
                    int other = links[j];
                    if (other < 0 || other >= _renderedGalaxy.Length)
                        continue;

                    int cidB = _renderedGalaxy[other].ConstellationId;
                    if (cidB <= 0 || cidA == cidB)
                        continue;

                    neighbors[cidA].Add(cidB);
                    neighbors[cidB].Add(cidA);
                }
            }

            // Разные диапазоны оттенков для соседних созвездий.
            float[] bandCenters = { 0.02f, 0.12f, 0.25f, 0.42f, 0.58f, 0.72f, 0.85f };
            float bandJitter = 0.025f;
            int bandCount = bandCenters.Length;

            var constellationIds = new List<int>();
            for (int i = 1; i <= maxId; i++)
                if (neighbors[i].Count > 0 || HasConstellation(i))
                    constellationIds.Add(i);

            constellationIds.Sort((a, b) => neighbors[b].Count.CompareTo(neighbors[a].Count));

            var bands = new int[maxId + 1];
            for (int i = 0; i < bands.Length; i++)
                bands[i] = -1;

            for (int i = 0; i < constellationIds.Count; i++)
            {
                int cid = constellationIds[i];
                var used = new bool[bandCount];
                foreach (var n in neighbors[cid])
                {
                    int nb = bands[n];
                    if (nb >= 0 && nb < bandCount)
                        used[nb] = true;
                }

                int chosen = -1;
                for (int b = 0; b < bandCount; b++)
                {
                    if (!used[b])
                    {
                        chosen = b;
                        break;
                    }
                }

                if (chosen < 0)
                {
                    int bestBand = 0;
                    int bestConflicts = int.MaxValue;
                    for (int b = 0; b < bandCount; b++)
                    {
                        int conflicts = 0;
                        foreach (var n in neighbors[cid])
                            if (bands[n] == b)
                                conflicts++;
                        if (conflicts < bestConflicts)
                        {
                            bestConflicts = conflicts;
                            bestBand = b;
                        }
                    }
                    chosen = bestBand;
                }

                bands[cid] = chosen;
            }

            _constellationColors = new Color[maxId + 1];
            for (int i = 1; i <= maxId; i++)
            {
                int band = bands[i];
                float hue = (band >= 0 && band < bandCount) ? bandCenters[band] : Hash01(i);
                float jitter = (Hash01(i * 92821) - 0.5f) * 2f * bandJitter;
                hue = Mathf.Repeat(hue + jitter + 1f, 1f);

                var color = Color.HSVToRGB(hue, constellationSaturation, constellationValue);
                color.a = constellationAlpha;
                _constellationColors[i] = color;
            }
        }

        private bool HasConstellation(int id)
        {
            for (int i = 0; i < _renderedGalaxy.Length; i++)
            {
                if (_renderedGalaxy[i].ConstellationId == id)
                    return true;
            }
            return false;
        }
    }
}
