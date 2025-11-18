using System.Collections.Generic;
using _Project.Prefabs;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.Render;
using UnityEngine;

namespace _Project.Scripts.SystemMap
{
    /// <summary>
    /// Renders static geometry of the system map: star, planets, moons and their orbits.
    /// Works as a layer consumed by <see cref="SystemMapRenderer"/>.
    /// </summary>
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
         private float orbitUnitPlanet = 10f;
        private float orbitUnitMoon = 1.5f;

        [Header("Base scale factors")]
        private float baseStarScale = 10f;
        private float basePlanetScale = 1.5f;
        private float baseMoonScale = 0.2f;
        private float basePlanetOrbitScale = 1f;
        private float baseMoonOrbitScale = 1f;

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
            }

            EnsureMaterial();
            ClearAll();
        }

        public void Render(
            in StarSys system,
            Ship[] prevShips,
            int prevCount,
            Ship[] currShips,
            int currCount,
            Ship[] nextShips,
            int nextCount,
            float progress,
            float stepDuration,
            System.Collections.Generic.IReadOnlyDictionary<_Project.Scripts.Core.UID, System.Collections.Generic.List<_Project.Scripts.Simulation.Render.SubstepSample>> substeps)
        {
            if (_layerRoot == null)
                return;

            ClearAll();
            DrawStar(system);
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

            var scale = Mathf.Max(0.0001f, baseStarScale * _starScaleOverride);
            starGo.transform.localScale = starGo.transform.localScale * scale;
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
                    orbitUnitPlanet *
                    Mathf.Max(0.0001f, basePlanetOrbitScale * _planetOrbitScaleOverride);

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
                    var planetScale = Mathf.Max(0.0001f, basePlanetScale * _planetScaleOverride);
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
                    orbitUnitMoon *
                    Mathf.Max(0.0001f, baseMoonOrbitScale * _moonOrbitScaleOverride);

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
                var moonScale = Mathf.Max(0.0001f, baseMoonScale * _moonScaleOverride);
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
