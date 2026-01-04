using System.Collections.Generic;
using _Project.Prefabs;
using _Project.Scripts.Const;
using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState.GameStateMembers.SelectedObj;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Selection;
using UnityEngine;

namespace _Project.Scripts.SystemMap
{
    /// <summary>Отрисовывает статическую геометрию карты системы: звезда, планеты, луны и орбиты.</summary>
    public sealed class SystemMapGeoRenderer : MonoBehaviour, ISystemMapLayer
    {
        [Header("Render order")]
        [SerializeField] private int order = 0;
        public int Order => order;

        [Header("Orbit rendering")]
        [SerializeField] private Material orbitMaterial;
        [SerializeField] private Color planetOrbitColor = new(0.6f, 0.8f, 1f, 0.35f);
        [SerializeField] private Color moonOrbitColor = new(1f, 1f, 1f, 0.18f);

        [Header("Orbit geometry")]
        private int segments = 128;

        [Header("Base scale factors")]

        [Header("Line width settings")]
        [SerializeField] private float lineWidthAtRefZoom = 0.015f;
        [SerializeField] private float referenceOrthoSize = 10f;
        [SerializeField] private Camera targetCamera;

        [Header("Prefab catalog")]
        [SerializeField] private PrefabCatalog catalog;

        private Transform _layerRoot;
        private Transform _starRoot;
        private Transform _planetOrbitsRoot;
        private Transform _moonOrbitsRoot;
        private Transform _planetsRoot;
        private Transform _deadZoneRoot;

        private readonly List<LineRenderer> _allOrbitLines = new();
        private float _starScaleOverride = 1f;
        private float _planetScaleOverride = 1f;
        private float _moonScaleOverride = 1f;
        private float _planetOrbitScaleOverride = 1f;
        private float _moonOrbitScaleOverride = 1f;

        public void Init(Transform parentRoot)
        {
            EnsureCamera();

            if (!_layerRoot)
            {
                _layerRoot = CreateRoot("GeoLayer", parentRoot);
                _starRoot = CreateRoot("StarRoot", _layerRoot);
                _planetOrbitsRoot = CreateRoot("PlanetOrbits", _layerRoot);
                _moonOrbitsRoot = CreateRoot("MoonOrbits", _layerRoot);
                _planetsRoot = CreateRoot("Planets", _layerRoot);
                _deadZoneRoot = CreateRoot("DeadZones", _layerRoot);
            }

            EnsureMaterial();
            ClearAll();
        }

        public void Render(in StarSys system)
        {
            if (_layerRoot == null)
                return;

            ClearAll();
            DrawStar(system);
            DrawDeadZones();
            DrawPlanetsAndMoons(system);
            UpdateLineWidthsImmediate();
        }

        public void Dispose() => ClearAll();

        public void SetScaleOverrides(
            float starScale,
            float planetScale,
            float moonScale,
            float planetOrbitScale,
            float moonOrbitScale)
        {
            _starScaleOverride = Mathf.Max(0.0001f, starScale);
            _planetScaleOverride = Mathf.Max(0.0001f, planetScale);
            _moonScaleOverride = Mathf.Max(0.0001f, moonScale);
            _planetOrbitScaleOverride = Mathf.Max(0.0001f, planetOrbitScale);
            _moonOrbitScaleOverride = Mathf.Max(0.0001f, moonOrbitScale);
        }

        private void DrawStar(in StarSys system)
        {
            var starPrefab = GetStarPrefab(system.Star.type);
            if (!starPrefab)
                return;

            var starGo = Instantiate(starPrefab, _starRoot);
            var starName = system.Star.Name;
            starGo.name = string.IsNullOrWhiteSpace(starName)
                ? $"Star_{system.Star.type}"
                : starName;
            starGo.transform.localPosition = Vector3.zero;
            var starSelectable = starGo.GetComponent<SelectableData>();
            if (starSelectable != null)
                starSelectable.SetData(GameBootstrap.GameState.SelectedSystemIndex, system.Star.Uid, ESelectedObjectType.Star);

            float scale = StarSysemConstants.StarPrefabScale * Mathf.Max(0.0001f, _starScaleOverride);
            starGo.transform.localScale = starGo.transform.localScale * Mathf.Max(0.0001f, scale);
        }

        private void DrawDeadZones()
        {
            if (!_deadZoneRoot || !orbitMaterial)
                return;

            float orbitUnit = _Project.Scripts.Galaxy.Config.OrbitMath.PlanetOrbitIndexToUnits(1);
            float innerRadius = Mathf.Max(0f, StarSysemConstants.InnerDeadZoneOrbits * orbitUnit);
            if (innerRadius <= 0f)
                return;

            var line = CreateCircle(_deadZoneRoot, Vector3.zero, innerRadius, Color.red);
            line.widthMultiplier = 0.04f;
            _allOrbitLines.Add(line);
        }

        private void DrawPlanetsAndMoons(in StarSys system)
        {
            var planets = system.PlanetSysArr;
            if (planets == null)
                return;

            for (int i = 0; i < planets.Length; i++)
            {
                var planetSys = planets[i];

                float orbitRadius =
                    Mathf.Max(0, planetSys.OrbitIndex) *
                    StarSysemConstants.PlanetOrbitUnit *
                    Mathf.Max(0.0001f, StarSysemConstants.PlanetOrbitScale * _planetOrbitScaleOverride);

                float angle = planetSys.OrbitPosition;
                Vector3 planetPos = new(Mathf.Cos(angle) * orbitRadius, Mathf.Sin(angle) * orbitRadius, 0f);

                var planetOrbit = CreateCircle(_planetOrbitsRoot, Vector3.zero, orbitRadius, planetOrbitColor);
                _allOrbitLines.Add(planetOrbit);

                var planetPrefab = GetPlanetPrefab(planetSys.Planet.Type);
                if (planetPrefab)
                {
                    var planetGo = Instantiate(planetPrefab, _planetsRoot);
                    var planetName = planetSys.Planet.Name;
                    planetGo.name = string.IsNullOrWhiteSpace(planetName)
                        ? $"Planet_{i}_{planetSys.Planet.Type}_Orbit{planetSys.OrbitIndex}"
                        : planetName;
                    planetGo.transform.localPosition = planetPos;
                    var planetSelectable = planetGo.GetComponent<SelectableData>();
                    if (planetSelectable != null)
                        planetSelectable.SetData(GameBootstrap.GameState.SelectedSystemIndex, planetSys.Planet.Uid, ESelectedObjectType.Planet);
                    float planetScale = Mathf.Max(
                        0.0001f,
                        StarSysemConstants.PlanetPrefabScale
                        * _planetScaleOverride);
                    planetGo.transform.localScale = planetGo.transform.localScale * planetScale;
                }

                DrawMoonsForPlanet(i, planetSys, planetPos, _planetsRoot, _moonOrbitsRoot);
            }
        }

        private void DrawMoonsForPlanet(
            int planetIndex,
            PlanetSys planetSys,
            Vector3 planetPos,
            Transform moonsRoot,
            Transform moonOrbitsRoot)
        {
            if (planetSys.Moons == null || planetSys.Moons.Length == 0)
                return;

            var moonRoot = new GameObject($"Moons_Planet_{planetIndex}").transform;
            moonRoot.SetParent(moonsRoot, false);
            moonRoot.localPosition = planetPos;

            var orbitRoot = new GameObject($"MoonOrbits_Planet_{planetIndex}").transform;
            orbitRoot.SetParent(moonOrbitsRoot, false);
            orbitRoot.localPosition = planetPos;

            for (int k = 0; k < planetSys.Moons.Length; k++)
            {
                var moon = planetSys.Moons[k];
                int orbitIndex = Mathf.Max(0, moon.OrbitIndex);
                if (orbitIndex <= 0)
                    continue;

                float orbitRadius =
                    orbitIndex *
                    StarSysemConstants.MoonOrbitUnit *
                    Mathf.Max(0.0001f, StarSysemConstants.MoonOrbitScale * _moonOrbitScaleOverride);

                var moonOrbit = CreateCircle(orbitRoot, Vector3.zero, orbitRadius, moonOrbitColor);
                _allOrbitLines.Add(moonOrbit);

                float angle = Hash01((planetIndex + 1) * 73856093 ^
                                     (k + 1) * 19349663 ^
                                     orbitIndex * 83492791) * Mathf.PI * 2f;

                Vector3 localPos = new(Mathf.Cos(angle) * orbitRadius, Mathf.Sin(angle) * orbitRadius, 0f);

                var moonPrefab = GetMoonPrefab(moon.Type);
                if (!moonPrefab)
                    continue;

                var moonGo = Instantiate(moonPrefab, moonRoot);
                var moonName = moon.Name;
                moonGo.name = string.IsNullOrWhiteSpace(moonName)
                    ? $"Moon_{planetIndex}_{k}_{moon.Type}_O{orbitIndex}"
                    : moonName;
                moonGo.transform.localPosition = localPos;
                var moonSelectable = moonGo.GetComponent<SelectableData>();
                if (moonSelectable != null)
                    moonSelectable.SetData(GameBootstrap.GameState.SelectedSystemIndex, moon.Uid, ESelectedObjectType.Moon);
                float moonScale = Mathf.Max(
                    0.0001f,
                    moon.Radius
                    * StarSysemConstants.MoonPrefabScale
                    * _moonScaleOverride);
                moonGo.transform.localScale = moonGo.transform.localScale * moonScale;
            }
        }

        private void EnsureCamera()
        {
            if (!targetCamera)
                targetCamera = Camera.main;
        }

        private void EnsureMaterial()
        {
            if (orbitMaterial)
                return;

            var shader = Shader.Find("Sprites/Default");
            orbitMaterial = new Material(shader) { color = Color.white };
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

        private static void ClearChildren(Transform target)
        {
            if (!target)
                return;

            for (int i = target.childCount - 1; i >= 0; i--)
            {
                var child = target.GetChild(i);
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }

        private GameObject GetStarPrefab(EStarType type)
        {
            if (!catalog || catalog.StarSystemPrefabsByType == null)
                return null;

            var index = (int)type;
            if (index < 0 || index >= catalog.StarSystemPrefabsByType.Length)
                return null;

            return catalog.StarSystemPrefabsByType[index];
        }

        private GameObject GetPlanetPrefab(EPlanetType type)
        {
            if (!catalog || catalog.PlanetPrefabsByType == null)
                return null;

            var index = (int)type;
            if (index < 0 || index >= catalog.PlanetPrefabsByType.Length)
                return null;

            return catalog.PlanetPrefabsByType[index];
        }

        private GameObject GetMoonPrefab(EMoonType type)
        {
            if (!catalog || catalog.MoonPrefabsByType == null)
                return null;

            var index = (int)type;
            if (index < 0 || index >= catalog.MoonPrefabsByType.Length)
                return null;

            return catalog.MoonPrefabsByType[index];
        }

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

            var points = new Vector3[segments];
            float twoPi = Mathf.PI * 2f;
            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments;
                float angle = twoPi * t;
                points[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            }

            lr.SetPositions(points);
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
            if (_allOrbitLines.Count == 0)
                return;

            if (!targetCamera)
                return;

            float camOrtho = Mathf.Max(0.0001f, targetCamera.orthographicSize);
            float width = lineWidthAtRefZoom * (camOrtho / referenceOrthoSize);

            for (int i = 0; i < _allOrbitLines.Count; i++)
            {
                var lr = _allOrbitLines[i];
                if (!lr)
                    continue;

                lr.widthMultiplier = width;
            }
        }

        private void UpdateLineWidthsImmediate()
        {
            if (!targetCamera)
                return;

            float camOrtho = Mathf.Max(0.0001f, targetCamera.orthographicSize);
            float width = lineWidthAtRefZoom * (referenceOrthoSize / camOrtho);

            for (int i = 0; i < _allOrbitLines.Count; i++)
            {
                var lr = _allOrbitLines[i];
                if (!lr)
                    continue;

                lr.widthMultiplier = width;
            }
        }
    }
}
