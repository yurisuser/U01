using _Project.Scripts.Galaxy.Data;
using UnityEngine;

namespace _Project.Scripts.SystemMap
{
    [DisallowMultipleComponent]
    public class SystemMapRenderer : MonoBehaviour
    {
        [Header("Префабы звёзд (можно те же, что на GalaxyMap)")]
        [SerializeField] private GameObject redPrefab;
        [SerializeField] private GameObject orangePrefab;
        [SerializeField] private GameObject yellowPrefab;
        [SerializeField] private GameObject whitePrefab;
        [SerializeField] private GameObject bluePrefab;
        [SerializeField] private GameObject neutronPrefab;
        [SerializeField] private GameObject blackPrefab;
        [SerializeField] private GameObject defaultStarPrefab;

        [Header("Планеты и орбиты")]
        [SerializeField] private GameObject planetPrefab;   // простая Sphere
        [SerializeField] private Material orbitMaterial;    // Unlit/Color предпочтительно
        [SerializeField, Range(16, 512)] private int orbitSegments = 128;

        [Header("Параметры раскладки")]
        [SerializeField] private float starScale = 1.5f;       // множитель масштаба звезды
        [SerializeField] private float firstOrbitRadius = 3f;  // радиус первой орбиты (юниты сцены)
        [SerializeField] private float orbitStep = 2f;         // шаг между орбитами
        [SerializeField] private float planetScale = 0.6f;     // размер шарика-планеты

        [SerializeField] private Transform root;

        void Awake()
        {
            if (!root)
            {
                var r = new GameObject("SystemRoot");
                r.transform.SetParent(transform, false);
                root = r.transform;
            }
        }

        void Start()
        {
            // 1) достаём систему
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

            // 2) отрисовываем
            float maxR = DrawSystem(sys);

            // 3) вписать в камеру
            var cam = Camera.main ? Camera.main.GetComponent<SystemMapCameraController>() : null;
            if (cam) cam.Frame(maxR);
        }

        float DrawSystem(StarSys sys)
        {
            // звезда в центре
            var starPrefab = GetStarPrefab(sys.Star.type) ?? defaultStarPrefab;
            if (!starPrefab) { Debug.LogWarning("[SystemMap] Нет префаба звезды."); return 5f; }

            var star = Instantiate(starPrefab, Vector3.zero, Quaternion.identity, root);
            star.name = string.IsNullOrWhiteSpace(sys.Name) ? "Star" : sys.Name;
            star.transform.localScale *= starScale;

            // планеты (пока по простому: равные интервалы)
            int count = sys.PlanetSysArr != null ? sys.PlanetSysArr.Length : 0;
            if (count <= 0) count = 4; // заглушки, если нет данных

            float maxRadius = 0f;
            for (int i = 0; i < count; i++)
            {
                float r = firstOrbitRadius + i * orbitStep;
                maxRadius = Mathf.Max(maxRadius, r);

                CreateOrbit(r);

                // планета на 0 градусов
                var planet = planetPrefab
                    ? Instantiate(planetPrefab, new Vector3(r, 0f, 0f), Quaternion.identity, root)
                    : GameObject.CreatePrimitive(PrimitiveType.Sphere);

                if (!planetPrefab) planet.transform.SetParent(root, true);
                planet.name = $"Planet_{i + 1}";
                planet.transform.localScale = Vector3.one * planetScale;
            }

            return maxRadius;
        }

        void CreateOrbit(float radius)
        {
            var go = new GameObject($"Orbit_{radius:0.0}");
            go.transform.SetParent(root, false);

            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.positionCount = orbitSegments;
            lr.widthMultiplier = 0.02f;

            if (orbitMaterial)
                lr.material = orbitMaterial;
            else
            {
                // простой серый, если материал не задан
                var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                mat.SetColor("_BaseColor", new Color(1f, 1f, 1f, 0.4f));
                lr.material = mat;
            }

            float step = 2f * Mathf.PI / orbitSegments;
            for (int i = 0; i < orbitSegments; i++)
            {
                float a = i * step;
                lr.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f));
            }
        }

        GameObject GetStarPrefab(StarType t)
        {
            switch (t)
            {
                case StarType.Red:     return redPrefab;
                case StarType.Orange:  return orangePrefab;
                case StarType.Yellow:  return yellowPrefab;
                case StarType.White:   return whitePrefab;
                case StarType.Blue:    return bluePrefab;
                case StarType.Neutron: return neutronPrefab;
                case StarType.Black:   return blackPrefab;
                default:               return defaultStarPrefab;
            }
        }
    }
}
