using System.Collections.Generic;
using _Project.Scripts.Galaxy.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using _Project.Scripts.Core.Scene;

namespace _Project.Scripts.SystemMap
{
    /// <summary>
    /// Рисует звезду, орбиты планет и орбиты лун (луны сами не рисуем).
    /// Всё в одной плоскости (Z=0). Геометрия строится один раз; в LateUpdate
    /// лишь подстраиваем widthMultiplier LineRenderer для постоянной экранной толщины.
    /// </summary>
    public class SystemMapRenderer : MonoBehaviour
    {
        
        private bool _isExiting;
        private Core.Core _core; 
        
        [Header("Материал и цвет орбит")]
        [SerializeField] private Material orbitMaterial;
        [SerializeField] private Color planetOrbitColor = new Color(0.6f, 0.8f, 1f, 0.35f);
        [SerializeField] private Color moonOrbitColor   = new Color(1f, 1f, 1f, 0.18f);

        [Header("Геометрия окружностей")]
        [SerializeField, Min(16)] private int segments = 128;
        [SerializeField] private float orbitUnitPlanet = 10f;   // радиус планетной орбиты = OrbitIndex * это
        [SerializeField] private float orbitUnitMoon   = 1.5f;  // радиус лунной орбиты   = OrbitIndex * это

        [Header("Экранная толщина линий (без шейдера)")]
        [SerializeField] private float lineWidthAtRefZoom = 0.015f;
        [SerializeField] private float referenceOrthoSize = 10f;
        [SerializeField] private Camera targetCamera; // если пусто — возьмём Camera.main

        [Header("Префабы (мэпы по типам)")]
        [Tooltip("Индекс = (int)StarType. Если элемент null — звезду не рисуем.")]
        [SerializeField] private GameObject[] starPrefabsByType;
        [Tooltip("Индекс = (int)PlanetType. Если элемент null — планету не рисуем.")]
        [SerializeField] private GameObject[] planetPrefabsByType;
        [Tooltip("Индекс = (int)MoonType. Если элемент null — планету не рисуем.")]
        [SerializeField] private GameObject[] moonPrefabsByType;

        // создаём эти руты сами — никаких ссылок не требуется
        private Transform _starRoot;
        private Transform _planetOrbitsRoot;
        private Transform _moonOrbitsRoot;
        private Transform _planetsRoot;

        // все LineRenderer’ы — чтобы регулировать толщину без пересборки
        private readonly List<LineRenderer> _allOrbitLines = new();

        // ============ ПУБЛИЧНО ============
        void Start()
        {
            StarSys sys;
            if (SelectedSystemBus.HasValue) sys = SelectedSystemBus.Selected;
            else
            {
                var galaxy = Core.Core.Galaxy;
                if (galaxy == null || galaxy.Length == 0)
                {
                    Debug.LogWarning("[SystemMap] Нет данных системы.");
                    return;
                }
                sys = galaxy[0];
            }

            DrawSystem(sys);
        }
        private void OnEnable()
        {
            if (_core?.Input != null)
                _core.Input.Subscribe(Key.Escape, OnEscPressed);
        }

        private void OnDisable()
        {
            if (_core?.Input != null)
                _core.Input.Unsubscribe(Key.Escape, OnEscPressed);
        }

        private void Awake()
        {
            _core = FindFirstObjectByType<Core.Core>();
        }

        
        private async void OnEscPressed()
        {
            if (_isExiting) return;
            _isExiting = true;

            await SceneController.LoadAsync(SceneId.GalaxyMap);
        }

        public void DrawSystem(StarSys system)
        {
            EnsureCamera();
            EnsureMaterial();
            EnsureRoots();

            ClearChildren(_starRoot);
            ClearChildren(_planetOrbitsRoot);
            ClearChildren(_moonOrbitsRoot);
            ClearChildren(_planetsRoot);
            _allOrbitLines.Clear();

            // 1) Звезда
            var starPrefab = GetStarPrefab(system.Star.type); // StarSys.Star.type — по твоим структурам :contentReference[oaicite:3]{index=3}
            if (starPrefab != null)
            {
                var starGo = Instantiate(starPrefab, _starRoot);
                starGo.name = $"Star_{system.Star.type}";
                starGo.transform.localPosition = Vector3.zero;
            }

            // 2) Планеты, их орбиты и орбиты лун
            var arr = system.PlanetSysArr; // именно PlanetSysArr :contentReference[oaicite:4]{index=4}
            if (arr != null)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    PlanetSys ps = arr[i]; // OrbitIndex/OrbitPosition/Moons — PascalCase :contentReference[oaicite:5]{index=5}

                    // радиус планетной орбиты
                    float rPlanet = Mathf.Max(0, ps.OrbitIndex) * orbitUnitPlanet;

                    // угловая позиция уже в данных (радианы)
                    float ang = ps.OrbitPosition;
                    Vector3 planetPos = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * rPlanet;

                    // кольцо орбиты планеты (рисуем только занятые орбиты → этот круг)
                    var planetOrbit = CreateCircle(_planetOrbitsRoot, Vector3.zero, rPlanet, planetOrbitColor);
                    _allOrbitLines.Add(planetOrbit);

                    // сама планета (по типу)
                    var planetPrefab = GetPlanetPrefab(ps.Planet.Type); // Planet.Type — PascalCase :contentReference[oaicite:6]{index=6}
                    if (planetPrefab != null)
                    {
                        var pGo = Instantiate(planetPrefab, _planetsRoot);
                        pGo.name = $"Planet_{i}_{ps.Planet.Type}_Orbit{ps.OrbitIndex}";
                        pGo.transform.localPosition = planetPos;
                    }

                    // орбиты лун вокруг планеты (луны не рисуем)
                    DrawMoonsForPlanet(i, ps, planetPos, _planetsRoot, _moonOrbitsRoot);
                }
            }

            // первичная установка толщины (на случай, если камера уже не ref-зума)
            UpdateLineWidthsImmediate();
        }
// Рисуем орбиты лун и сами луны вокруг планеты.
// planetIndex — индекс планеты в системе (для детерминированного угла)
// ps          — PlanetSys (в нём Moons[] с OrbitIndex/Type)
// planetPos   — позиция планеты в локале карты
// moonsRoot   — родительский узел для объектов лун (GameObject’ы лун)
// moonOrbitsRoot — родительский узел для линий орбит лун
private void DrawMoonsForPlanet(
    int planetIndex,
    PlanetSys ps,
    Vector3 planetPos,
    Transform moonsRoot,
    Transform moonOrbitsRoot)
{
    if (ps.Moons == null || ps.Moons.Length == 0) return;

    // локальный центр лун у этой планеты
    var center = new GameObject($"Moons_Planet_{planetIndex}").transform;
    center.SetParent(moonsRoot, false);
    center.localPosition = planetPos;

    // локальный корень для линий орбит лун
    var orbitsCenter = new GameObject($"MoonOrbits_Planet_{planetIndex}").transform;
    orbitsCenter.SetParent(moonOrbitsRoot, false);
    orbitsCenter.localPosition = planetPos;

    for (int k = 0; k < ps.Moons.Length; k++)
    {
        var moon = ps.Moons[k];
        int orbitIdx = Mathf.Max(0, moon.OrbitIndex);
        if (orbitIdx <= 0) continue;

        // 1) Орбита луны (кольцо) — тот же круговой LineRenderer, что и для планет
        float rMoon = orbitIdx * orbitUnitMoon;                // <-- поле класса
        var moonOrbit = CreateCircle(orbitsCenter, Vector3.zero, rMoon, moonOrbitColor);
        _allOrbitLines.Add(moonOrbit);

        // 2) Позиция самой луны: детерминированный угол по индексам, без анимации
        float angle = Hash01((planetIndex + 1) * 73856093 ^ (k + 1) * 19349663 ^ orbitIdx * 83492791) * (Mathf.PI * 2f);
        Vector3 local = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * rMoon;

        // 3) Инстансим префаб луны по типу (если указан); иначе — пропускаем спрайт, остаётся только орбита
        var moonPrefab = GetMoonPrefab(moon.Type);             // <-- ожидается массив префабов по (int)MoonType
        if (moonPrefab != null)
        {
            var mGo = Instantiate(moonPrefab, center);
            mGo.name = $"Moon_{planetIndex}_{k}_{moon.Type}_O{orbitIdx}";
            mGo.transform.localPosition = local;
        }
    }
}

// ——— Хелпер для детерминированного угла [0..1)
private static float Hash01(int seed)
{
    unchecked
    {
        uint x = (uint)seed;
        x ^= x >> 17; x *= 0xED5AD4BBu;
        x ^= x >> 11; x *= 0xAC4C1B51u;
        x ^= x >> 15; x *= 0x31848BABu;
        x ^= x >> 14;
        return (x & 0xFFFFFFu) / 16777216f; // 2^24
    }
}

// ——— Ожидается хелпер у класса; если его нет, добавь аналогично GetPlanetPrefab
private GameObject GetMoonPrefab(MoonType type)
{
    int idx = (int)type;
    if (moonPrefabsByType == null || idx < 0 || idx >= moonPrefabsByType.Length) return null;
    return moonPrefabsByType[idx];
}

        // ============ ВСПОМОГАТЕЛЬНО ============

        private void EnsureCamera()
        {
            if (!targetCamera) targetCamera = Camera.main;
        }

        private void EnsureMaterial()
        {
            if (!orbitMaterial)
            {
                // простой дефолт без шейдера-кастом: годится для LineRenderer
                var shader = Shader.Find("Sprites/Default");
                orbitMaterial = new Material(shader) { color = Color.white };
            }
        }

        private void EnsureRoots()
        {
            if (!_starRoot)        _starRoot        = CreateRoot("StarRoot");
            if (!_planetOrbitsRoot)_planetOrbitsRoot= CreateRoot("PlanetOrbits");
            if (!_moonOrbitsRoot)  _moonOrbitsRoot  = CreateRoot("MoonOrbits");
            if (!_planetsRoot)     _planetsRoot     = CreateRoot("Planets");
        }

        private Transform CreateRoot(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.zero;
            return go.transform;
        }

        private void ClearChildren(Transform t)
        {
            if (!t) return;
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                var c = t.GetChild(i);
                if (Application.isPlaying) Destroy(c.gameObject);
                else DestroyImmediate(c.gameObject);
            }
        }

        private GameObject GetStarPrefab(StarType type)
        {
            int idx = (int)type;
            if (starPrefabsByType == null || idx < 0 || idx >= starPrefabsByType.Length) return null;
            return starPrefabsByType[idx];
        }

        private GameObject GetPlanetPrefab(PlanetType type)
        {
            int idx = (int)type;
            if (planetPrefabsByType == null || idx < 0 || idx >= planetPrefabsByType.Length) return null;
            return planetPrefabsByType[idx];
        }

        private LineRenderer CreateCircle(Transform parent, Vector3 center, float radius, Color color)
        {
            var go = new GameObject("Orbit");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = center;

            var lr = go.AddComponent<LineRenderer>();
            lr.sharedMaterial = orbitMaterial;
            lr.loop = true;
            lr.useWorldSpace = false;          // центр круга = локальная позиция узла
            lr.alignment = LineAlignment.View; // в экран
            lr.textureMode = LineTextureMode.Stretch;
            lr.positionCount = segments;

            // временная толщина — подстроим в LateUpdate
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

        // поддерживаем постоянную экранную толщину линий (без шейдера, без пересборки)
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
