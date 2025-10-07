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
        [SerializeField] private Material orbitMaterial;    // Unlit/Color предпочтительно
        [SerializeField, Range(16, 512)] private int orbitSegments = 128;

        [Header("Параметры раскладки")]
        [SerializeField] private float starScale = 1.5f;
        [SerializeField] private float firstOrbitRadius = 3f;
        [SerializeField] private float orbitStep = 2f;
        [SerializeField] private float planetScale = 0.6f;

        [Header("Префабы планет по типам (индекс = PlanetType)")]
        [SerializeField] public GameObject[] planetPrefabs;

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

            float maxR = DrawSystem(sys);

            var cam = Camera.main ? Camera.main.GetComponent<SystemMapCameraController>() : null;
            if (cam) cam.Frame(maxR);
        }

        float DrawSystem(StarSys sys)
        {
            var starPrefab = GetStarPrefab(sys.Star.type) ?? defaultStarPrefab;
            if (!starPrefab)
            {
                Debug.LogWarning("[SystemMap] Нет префаба звезды.");
                return 5f;
            }

            var star = Instantiate(starPrefab, Vector3.zero, Quaternion.identity, root);
            star.name = string.IsNullOrWhiteSpace(sys.Name) ? "Star" : sys.Name;
            star.transform.localScale *= starScale;

            int count = sys.PlanetSysArr != null ? sys.PlanetSysArr.Length : 0;
            if (count <= 0) count = 4; // заглушки, если нет данных

            float maxRadius = 0f;
            for (int i = 0; i < count; i++)
            {
                float r = firstOrbitRadius + i * orbitStep;
                maxRadius = Mathf.Max(maxRadius, r);
                CreateOrbit(r);

                if (sys.PlanetSysArr == null || i >= sys.PlanetSysArr.Length)
                    continue;

                Planet planet = sys.PlanetSysArr[i].Planet;
                int pid = (int)planet.Type;

                if (pid < 0 || pid >= planetPrefabs.Length)
                    continue;

                var prefab = planetPrefabs[pid];
                if (prefab == null)
                    continue;

                var go = Instantiate(prefab, new Vector3(r, 0f, 0f), Quaternion.identity, root);
                go.name = $"Planet_{i + 1}_{planet.Type}";
                go.transform.localScale = Vector3.one * planetScale;
            }

            return maxRadius;
        }

        void CreateOrbit(float radius)
        {
            var go = new GameObject($"Orbit_{radius:F1}");
            go.transform.SetParent(root, false);

            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.alignment = LineAlignment.View;
            lr.textureMode = LineTextureMode.Stretch;

            // ---------- Материал ----------
            Material mat = orbitMaterial;
            if (mat == null || mat.shader == null)
            {
                // создаём безопасный фолбек-материал
                var sh = Shader.Find("Universal Render Pipeline/Unlit");
                if (sh == null)
                    sh = Shader.Find("Unlit/Color");

                mat = new Material(sh);
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", new Color(0.7f, 0.7f, 0.9f, 0.5f)); // слегка сиренево-серый, полупрозрачный
                else if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", new Color(0.7f, 0.7f, 0.9f, 0.5f));
            }
            lr.material = mat;

            // ---------- Визуальные настройки ----------
            lr.startWidth = 0.01f;
            lr.endWidth = 0.01f;
            lr.widthMultiplier = 1f;
            lr.numCornerVertices = 0;
            lr.numCapVertices = 0;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.allowOcclusionWhenDynamic = false;

            // ---------- Генерация точек орбиты ----------
            lr.positionCount = orbitSegments;
            Vector3[] pts = new Vector3[orbitSegments];
            for (int i = 0; i < orbitSegments; i++)
            {
                float ang = (float)i / orbitSegments * Mathf.PI * 2f;
                pts[i] = new Vector3(Mathf.Cos(ang) * radius, 0f, Mathf.Sin(ang) * radius);
            }

            lr.SetPositions(pts);
        }



        GameObject GetStarPrefab(StarType type)
        {
            switch (type)
            {
                case StarType.Red: return redPrefab;
                case StarType.Orange: return orangePrefab;
                case StarType.Yellow: return yellowPrefab;
                case StarType.White: return whitePrefab;
                case StarType.Blue: return bluePrefab;
                case StarType.Neutron: return neutronPrefab;
                case StarType.Black: return blackPrefab;
                default: return defaultStarPrefab;
            }
        }
    }
}
