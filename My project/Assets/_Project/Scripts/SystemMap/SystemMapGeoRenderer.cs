using System.Collections.Generic;
using _Project.Scripts.Galaxy.Data;
using UnityEngine;
// List<LineRenderer> — учёт всех орбит
// MonoBehaviour, Transform, Camera, Material, Color
// StarSys, PlanetSys

// ISystemMapLayer

namespace _Project.Scripts.SystemMap
{
    /// <summary>
    /// Географический слой карты системы: звезда, планеты, орбиты планет и лун.
    /// Реализует ISystemMapLayer и живёт под оркестратором SystemMapRenderer.
    /// </summary>
    public sealed class SystemMapGeoRenderer : MonoBehaviour, ISystemMapLayer
    {
        [Header("Порядок слоя")]
        [SerializeField] private int order = 0;                  // порядок рендера: гео рисуем раньше кораблей
        public int Order => order;

        [Header("Материал и цвета орбит")]
        [SerializeField] private Material orbitMaterial;         // материал для LineRenderer'ов
        [SerializeField] private Color planetOrbitColor = new(0.6f, 0.8f, 1f, 0.35f);
        [SerializeField] private Color moonOrbitColor   = new(1f, 1f, 1f, 0.18f);

        [Header("Геометрия окружностей")]
        [SerializeField, Min(16)] private int segments = 128;    // сегменты круга для орбит
        [SerializeField] private float orbitUnitPlanet = 10f;    // радиус планетной орбиты = OrbitIndex * это
        [SerializeField] private float orbitUnitMoon   = 1.5f;   // радиус лунной  орбиты = OrbitIndex * это

        [Header("Экранная толщина линий (без шейдера)")]
        [SerializeField] private float lineWidthAtRefZoom = 0.015f; // толщина при ref-зумах
        [SerializeField] private float referenceOrthoSize = 10f;    // опорный орто-зуум
        [SerializeField] private Camera targetCamera;               // если пусто — возьмём Camera.main

        [Header("Префабы (мэпы по типам)")]
        [Tooltip("Индекс = (int)EStarType. Если элемент null — звезду не рисуем.")]
        [SerializeField] private GameObject[] starPrefabsByType;
        [Tooltip("Индекс = (int)EPlanetType. Если элемент null — планету не рисуем.")]
        [SerializeField] private GameObject[] planetPrefabsByType;
        [Tooltip("Индекс = (int)EMoonType. Если элемент null — луну не рисуем.")]
        [SerializeField] private GameObject[] moonPrefabsByType;

        // Корни слоя (создаются под parentRoot из Init)
        private Transform _layerRoot;                             // корень всего слоя (GeoLayer)
        private Transform _starRoot;                              // звезда
        private Transform _planetOrbitsRoot;                      // линии орбит планет
        private Transform _moonOrbitsRoot;                        // линии орбит лун
        private Transform _planetsRoot;                           // объекты планет/лун

        // Учёт всех линий для быстрой смены толщины
        private readonly List<LineRenderer> _allOrbitLines = new();

        // ---------------- ISystemMapLayer ----------------

        public void Init(Transform parentRoot)                    // создаём структуру узлов и готовим материал/камеру
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

        public void Render(in StarSys sys)                        // полный дифф: на текущем этапе — простой пересбор
        {
            if (_layerRoot == null) return;

            ClearAll();                                           // простой путь: чистим и рисуем заново (пока без инкремента)
            DrawStar(sys);
            DrawPlanetsAndMoons(sys);

            UpdateLineWidthsImmediate();                          // первичная корректировка толщины
        }

        public void Dispose()                                     // очистка: вернуть всё в исходное состояние
        {
            ClearAll();
        }

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

                // радиус планетной орбиты
                float rPlanet = Mathf.Max(0, ps.OrbitIndex) * orbitUnitPlanet;

                // угловая позиция уже в данных (радианы)
                float ang = ps.OrbitPosition;
                Vector3 planetPos = new(Mathf.Cos(ang) * rPlanet, Mathf.Sin(ang) * rPlanet, 0f);

                // кольцо орбиты планеты
                var planetOrbit = CreateCircle(_planetOrbitsRoot, Vector3.zero, rPlanet, planetOrbitColor);
                _allOrbitLines.Add(planetOrbit);

                // планета по типу
                var planetPrefab = GetPlanetPrefab(ps.Planet.Type);
                if (planetPrefab != null)
                {
                    var pGo = Instantiate(planetPrefab, _planetsRoot);
                    pGo.name = $"Planet_{i}_{ps.Planet.Type}_Orbit{ps.OrbitIndex}";
                    pGo.transform.localPosition = planetPos;
                }

                // орбиты и позиции лун
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

                // 1) Орбита луны
                float rMoon = orbitIdx * orbitUnitMoon;
                var moonOrbit = CreateCircle(orbitsCenter, Vector3.zero, rMoon, moonOrbitColor);
                _allOrbitLines.Add(moonOrbit);

                // 2) Позиция луны: детерминированный угол
                float angle = Hash01((planetIndex + 1) * 73856093 ^ (k + 1) * 19349663 ^ orbitIdx * 83492791) * (Mathf.PI * 2f);
                Vector3 local = new(Mathf.Cos(angle) * rMoon, Mathf.Sin(angle) * rMoon, 0f);

                // 3) Префаб луны
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
            if (!targetCamera) targetCamera = Camera.main;        // если не задана — берём главную камеру
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

        private GameObject GetStarPrefab(EStarType type)
        {
            int idx = (int)type;
            if (starPrefabsByType == null || idx < 0 || idx >= starPrefabsByType.Length) return null;
            return starPrefabsByType[idx];
        }

        private GameObject GetPlanetPrefab(EPlanetType type)
        {
            int idx = (int)type;
            if (planetPrefabsByType == null || idx < 0 || idx >= planetPrefabsByType.Length) return null;
            return planetPrefabsByType[idx];
        }

        private GameObject GetMoonPrefab(EMoonType type)
        {
            int idx = (int)type;
            if (moonPrefabsByType == null || idx < 0 || idx >= moonPrefabsByType.Length) return null;
            return moonPrefabsByType[idx];
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

            // базовая толщина (пересчитаем сразу после рендера)
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

        private static float Hash01(int seed)                     // детерминированный [0..1)
        {
            unchecked
            {
                uint x = (uint)seed;
                x ^= x >> 17; x *= 0xED5AD4BBu;
                x ^= x >> 11; x *= 0xAC4C1B51u;
                x ^= x >> 15; x *= 0x31848BABu;
                x ^= x >> 14;
                return (x & 0xFFFFFFu) / 16777216f;               // 2^24
            }
        }

        private void LateUpdate()                                 // поддерживаем постоянную относит. толщину линий
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
