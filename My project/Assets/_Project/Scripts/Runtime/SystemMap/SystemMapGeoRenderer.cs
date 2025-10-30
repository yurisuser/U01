using System.Collections.Generic;
using _Project.Scripts.Galaxy.Data;
using UnityEngine;
using _Project.Prefabs;                                        // добавлено — для доступа к PrefabCatalog

namespace _Project.Scripts.SystemMap
{
    /// <summary>
    /// Географический слой карты системы: звезда, планеты, орбиты планет и лун.
    /// Реализует ISystemMapLayer и живёт под оркестратором SystemMapRenderer.
    /// </summary>
    public sealed class SystemMapGeoRenderer : MonoBehaviour, ISystemMapLayer
    {
        [Header("Порядок слоя")]
        [SerializeField] private int order = 0;
        public int Order => order;

        [Header("Материал и цвета орбит")]
        [SerializeField] private Material orbitMaterial;
        [SerializeField] private Color planetOrbitColor = new(0.6f, 0.8f, 1f, 0.35f);
        [SerializeField] private Color moonOrbitColor   = new(1f, 1f, 1f, 0.18f);

        [Header("Геометрия окружностей")]
        [SerializeField, Min(16)] private int segments = 128;
        [SerializeField] private float orbitUnitPlanet = 10f;
        [SerializeField] private float orbitUnitMoon   = 1.5f;

        [Header("Экранная толщина линий (без шейдера)")]
        [SerializeField] private float lineWidthAtRefZoom = 0.015f;
        [SerializeField] private float referenceOrthoSize = 10f;
        [SerializeField] private Camera targetCamera;

        [Header("Каталог префабов")]                           // добавлено
        [SerializeField] private PrefabCatalog catalog;         // добавлено

        // Корни слоя
        private Transform _layerRoot;
        private Transform _starRoot;
        private Transform _planetOrbitsRoot;
        private Transform _moonOrbitsRoot;
        private Transform _planetsRoot;

        private readonly List<LineRenderer> _allOrbitLines = new();

        // ---------------- ISystemMapLayer ----------------

        public void Init(Transform parentRoot)
        {
            EnsureCamera();

            if (!_layerRoot)
            {
                _layerRoot = CreateRoot("GeoLayer", parentRoot);
                _starRoot        = CreateRoot("StarRoot",        _layerRoot);
                _planetOrbitsRoot= CreateRoot("PlanetOrbits",    _layerRoot);
                _moonOrbitsRoot  = CreateRoot("MoonOrbits",      _layerRoot);
                _planetsRoot     = CreateRoot("Planets",         _layerRoot);
            }

            EnsureMaterial();
            ClearAll();
        }

        public void Render(in StarSys sys)
        {
            if (_layerRoot == null) return;

            ClearAll();
            DrawStar(sys);
            DrawPlanetsAndMoons(sys);
            UpdateLineWidthsImmediate();
        }

        public void Dispose() => ClearAll();

        // ---------------- Рисование ----------------

        private void DrawStar(in StarSys system)
        {
            var starPrefab = GetStarPrefab(system.Star.type);
            if (starPrefab == null) return;

            var starGo = Instantiate(starPrefab, _starRoot);
            starGo.name = $"Star_{system.Star.type}";
            starGo.transform.localPosition = Vector3.zero;
        }

        private void DrawPlanetsAndMoons(in StarSys system)
        {
            var arr = system.PlanetSysArr;
            if (arr == null) return;

            for (int i = 0; i < arr.Length; i++)
            {
                PlanetSys ps = arr[i];

                float rPlanet = Mathf.Max(0, ps.OrbitIndex) * orbitUnitPlanet;
                float ang = ps.OrbitPosition;
                Vector3 planetPos = new(Mathf.Cos(ang) * rPlanet, Mathf.Sin(ang) * rPlanet, 0f);

                var planetOrbit = CreateCircle(_planetOrbitsRoot, Vector3.zero, rPlanet, planetOrbitColor);
                _allOrbitLines.Add(planetOrbit);

                var planetPrefab = GetPlanetPrefab(ps.Planet.Type);
                if (planetPrefab != null)
                {
                    var pGo = Instantiate(planetPrefab, _planetsRoot);
                    pGo.name = $"Planet_{i}_{ps.Planet.Type}_Orbit{ps.OrbitIndex}";
                    pGo.transform.localPosition = planetPos;
                }

                DrawMoonsForPlanet(i, ps, planetPos, _planetsRoot, _moonOrbitsRoot);
            }
        }

        private void DrawMoonsForPlanet(
            int planetIndex,
            PlanetSys ps,
            Vector3 planetPos,
            Transform moonsRoot,
            Transform moonOrbitsRoot)
        {
            if (ps.Moons == null || ps.Moons.Length == 0) return;

            var center = new GameObject($"Moons_Planet_{planetIndex}").transform;
            center.SetParent(moonsRoot, false);
            center.localPosition = planetPos;

            var orbitsCenter = new GameObject($"MoonOrbits_Planet_{planetIndex}").transform;
            orbitsCenter.SetParent(moonOrbitsRoot, false);
            orbitsCenter.localPosition = planetPos;

            for (int k = 0; k < ps.Moons.Length; k++)
            {
                var moon = ps.Moons[k];
                int orbitIdx = Mathf.Max(0, moon.OrbitIndex);
                if (orbitIdx <= 0) continue;

                float rMoon = orbitIdx * orbitUnitMoon;
                var moonOrbit = CreateCircle(orbitsCenter, Vector3.zero, rMoon, moonOrbitColor);
                _allOrbitLines.Add(moonOrbit);

                float angle = Hash01((planetIndex + 1) * 73856093 ^ (k + 1) * 19349663 ^ orbitIdx * 83492791) * (Mathf.PI * 2f);
                Vector3 local = new(Mathf.Cos(angle) * rMoon, Mathf.Sin(angle) * rMoon, 0f);

                var moonPrefab = GetMoonPrefab(moon.Type);
                if (moonPrefab != null)
                {
                    var mGo = Instantiate(moonPrefab, center);
                    mGo.name = $"Moon_{planetIndex}_{k}_{moon.Type}_O{orbitIdx}";
                    mGo.transform.localPosition = local;
                }
            }
        }

        // ---------------- Технические хелперы ----------------

        private void EnsureCamera()
        {
            if (!targetCamera) targetCamera = Camera.main;
        }

        private void EnsureMaterial()
        {
            if (!orbitMaterial)
            {
                var shader = Shader.Find("Sprites/Default");
                orbitMaterial = new Material(shader) { color = Color.white };
            }
        }

        private Transform CreateRoot(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.zero;
            return go.transform;
        }

        private void ClearAll()
        {
            _allOrbitLines.Clear();
            ClearChildren(_starRoot);
            ClearChildren(_planetOrbitsRoot);
            ClearChildren(_moonOrbitsRoot);
            ClearChildren(_planetsRoot);
        }

        private static void ClearChildren(Transform t)
        {
            if (!t) return;
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                var c = t.GetChild(i);
                if (Application.isPlaying) Object.Destroy(c.gameObject);
                else Object.DestroyImmediate(c.gameObject);
            }
        }

        // === заменено: теперь читаем из PrefabCatalog ===

        private GameObject GetStarPrefab(EStarType type)
        {
            if (!catalog || catalog.StarSystemPrefabsByType == null) return null;
            int idx = (int)type;
            if (idx < 0 || idx >= catalog.StarSystemPrefabsByType.Length) return null;
            return catalog.StarSystemPrefabsByType[idx];
        }

        private GameObject GetPlanetPrefab(EPlanetType type)
        {
            if (!catalog || catalog.PlanetPrefabsByType == null) return null;
            int idx = (int)type;
            if (idx < 0 || idx >= catalog.PlanetPrefabsByType.Length) return null;
            return catalog.PlanetPrefabsByType[idx];
        }

        private GameObject GetMoonPrefab(EMoonType type)
        {
            if (!catalog || catalog.MoonPrefabsByType == null) return null;
            int idx = (int)type;
            if (idx < 0 || idx >= catalog.MoonPrefabsByType.Length) return null;
            return catalog.MoonPrefabsByType[idx];
        }

        // ================================================

        private LineRenderer CreateCircle(Transform parent, Vector3 center, float radius, Color color)
        {
            var go = new GameObject("Orbit");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = center;

            var lr = go.AddComponent<LineRenderer>();
            lr.sharedMaterial = orbitMaterial;
            lr.loop = true;
            lr.useWorldSpace = false;
            lr.alignment = LineAlignment.View;
            lr.textureMode = LineTextureMode.Stretch;
            lr.positionCount = segments;
            lr.widthMultiplier = lineWidthAtRefZoom;

            var pts = new Vector3[segments];
            float twoPi = Mathf.PI * 2f;
            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments;
                float a = twoPi * t;
                pts[i] = new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f);
            }
            lr.SetPositions(pts);
            lr.startColor = color;
            lr.endColor = color;

            return lr;
        }

        private static float Hash01(int seed)
        {
            unchecked
            {
                uint x = (uint)seed;
                x ^= x >> 17; x *= 0xED5AD4BBu;
                x ^= x >> 11; x *= 0xAC4C1B51u;
                x ^= x >> 15; x *= 0x31848BABu;
                x ^= x >> 14;
                return (x & 0xFFFFFFu) / 16777216f;
            }
        }

        private void LateUpdate()
        {
            if (_allOrbitLines.Count == 0) return;
            if (!targetCamera) return;

            float camOrtho = Mathf.Max(0.0001f, targetCamera.orthographicSize);
            float width = lineWidthAtRefZoom * (camOrtho / referenceOrthoSize);

            for (int i = 0; i < _allOrbitLines.Count; i++)
            {
                var lr = _allOrbitLines[i];
                if (!lr) continue;
                lr.widthMultiplier = width;
            }
        }

        private void UpdateLineWidthsImmediate()
        {
            if (!targetCamera) return;
            float camOrtho = Mathf.Max(0.0001f, targetCamera.orthographicSize);
            float width = lineWidthAtRefZoom * (referenceOrthoSize / camOrtho);
            for (int i = 0; i < _allOrbitLines.Count; i++)
            {
                var lr = _allOrbitLines[i];
                if (!lr) continue;
                lr.widthMultiplier = width;
            }
        }
    }
}
