#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem; // новый Input System
#endif
using System.Collections.Generic;
using UnityEngine;
using _Project.Galaxy.Obj; // StarSys, StarType, StarSize

namespace _Project.Components
{
    [DisallowMultipleComponent]
    public class GalaxyMapRenderer : MonoBehaviour
    {
        [Header("Префабы по типам звёзд")]
        [SerializeField] private GameObject redPrefab;
        [SerializeField] private GameObject orangePrefab;
        [SerializeField] private GameObject yellowPrefab;
        [SerializeField] private GameObject whitePrefab;
        [SerializeField] private GameObject bluePrefab;
        [SerializeField] private GameObject neutronPrefab;
        [SerializeField] private GameObject blackPrefab;
        [SerializeField] private GameObject defaultPrefab; // поставь сюда что-нибудь

        [Header("Скейл по размеру (множители)")]
        [SerializeField] private float dwarfMul      = 0.7f;
        [SerializeField] private float normalMul     = 1.0f;
        [SerializeField] private float giantMul      = 1.4f;
        [SerializeField] private float supergiantMul = 1.8f;

        [Header("Глобальный множитель")]
        [SerializeField] private float globalScale = 1.0f;

        [Header("Родитель и камера (камеру можно не трогать)")]
        [SerializeField] private Transform starsRoot;
        [SerializeField] private Camera targetCamera; // если пусто — возьмём MainCamera

        private readonly List<GameObject> _spawned = new();
        private readonly Dictionary<Collider, StarType> _typeByCollider = new();
        private readonly Dictionary<Collider, string>   _nameByCollider = new();

        // ---------- Lifecycle ----------
        private void Awake()
        {
            if (!starsRoot)
            {
                var root = new GameObject("StarsRoot");
                root.transform.SetParent(transform, false);
                starsRoot = root.transform;
            }
            EnsureCamera();
        }

        private void Start()
        {
            var galaxy = Core.Galaxy; // StarSys[]
            if (galaxy == null || galaxy.Length == 0)
            {
                Debug.LogWarning("[GalaxyMapRenderer] Core.Galaxy пуст. Нечего рисовать.");
                return;
            }
            Render(galaxy, clearBefore: true);
        }

        private void Update()
        {
            // ЛКМ (новый/старый ввод)
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            if (Mouse.current == null || !Mouse.current.leftButton.wasReleasedThisFrame) return;
#else
            if (!Input.GetMouseButtonUp(0)) return;
#endif
            var cam = EnsureCamera();
            if (!cam) return;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            Vector2 pos = Mouse.current.position.ReadValue();
            var ray = cam.ScreenPointToRay(pos);
#else
            var ray = cam.ScreenPointToRay(Input.mousePosition);
#endif
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, ~0))
            {
                if (_typeByCollider.TryGetValue(hit.collider, out var t))
                {
                    var nm = _nameByCollider.TryGetValue(hit.collider, out var n) ? n : hit.collider.gameObject.name;
                    Debug.Log($"[Star] {nm} → {t}");
                }
            }
        }

        // ---------- Public ----------
        public void Render(StarSys[] systems, bool clearBefore = true)
        {
            if (systems == null || systems.Length == 0) return;

            if (clearBefore)
            {
                for (int i = 0; i < _spawned.Count; i++)
                    if (_spawned[i]) Destroy(_spawned[i]);
                _spawned.Clear();
                _typeByCollider.Clear();
                _nameByCollider.Clear();
            }

            int missingPrefab = 0;
            int spawned = 0;

            for (int i = 0; i < systems.Length; i++)
            {
                var s = systems[i];

                // 1) Префаб по типу (+ фолбэк)
                var prefab = GetPrefabFor(s.Star.type) ?? defaultPrefab;

                // 2) Если вообще нет префабов — создаём заметный плейсхолдер (розовый шар)
                GameObject go;
                if (!prefab)
                {
                    missingPrefab++;
                    go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    go.GetComponent<Renderer>().material.color = Color.magenta;
                    go.name = $"PLACEHOLDER_{i:0000}";
                    go.transform.SetParent(starsRoot, false);
                    go.transform.position = s.GalaxyPosition;
                }
                else
                {
                    go = Instantiate(prefab, s.GalaxyPosition, Quaternion.identity, starsRoot);
                    go.name = string.IsNullOrWhiteSpace(s.Name) ? $"SYS-{i:0000}" : s.Name;
                }

                // 3) Скейл по размеру
                float mul = GetSizeMul(s.Star.size) * Mathf.Max(0.0001f, globalScale);
                go.transform.localScale *= mul;

                // 4) Коллайдер + слой
                go.layer = LayerMask.NameToLayer("Default");
                var col = go.GetComponent<Collider>() ?? go.AddComponent<SphereCollider>();
                if (col is SphereCollider sc)
                {
                    sc.isTrigger = false;
                    if (sc.radius <= 0.0001f) sc.radius = 0.5f;
                }

                // 5) Привязка для клика
                _typeByCollider[col] = s.Star.type;
                _nameByCollider[col] = go.name;

                _spawned.Add(go);
                spawned++;
            }

            Debug.Log($"[GalaxyMapRenderer] Отрисовано звёзд: {spawned}. " +
                      (missingPrefab > 0
                          ? $"Плейсхолдеров из-за пустых префабов: {missingPrefab} (заполни слоты в инспекторе)."
                          : "Все префабы найдены."));
        }

        // ---------- Helpers ----------
        private Camera EnsureCamera()
        {
            if (targetCamera && targetCamera.isActiveAndEnabled) return targetCamera;

            // 1) если скрипт висит на камере — берём её
            var selfCam = GetComponent<Camera>();
            if (selfCam && selfCam.isActiveAndEnabled) { targetCamera = selfCam; return targetCamera; }

            // 2) MainCamera по тегу
            var main = Camera.main;
            if (main && main.isActiveAndEnabled) { targetCamera = main; return targetCamera; }

            // 3) Любая активная камера в сцене (с учётом неактивных объектов)
            var cams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < cams.Length; i++)
            {
                if (cams[i] && cams[i].isActiveAndEnabled)
                {
                    targetCamera = cams[i];
                    return targetCamera;
                }
            }

            if (!targetCamera)
                Debug.LogWarning("[GalaxyMapRenderer] Камера не найдена. Повесь скрипт на камеру или укажи Target Camera.");
            return null;
        }


        private GameObject GetPrefabFor(StarType t)
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
                default:               return defaultPrefab;
            }
        }

        private float GetSizeMul(StarSize z)
        {
            switch (z)
            {
                case StarSize.Dwarf:       return dwarfMul;
                case StarSize.Normal:      return normalMul;
                case StarSize.Giant:       return giantMul;
                case StarSize.Supergiant:  return supergiantMul;
                default:                   return normalMul;
            }
        }
    }
}
