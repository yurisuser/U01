using UnityEngine;                                              // MonoBehaviour, Transform
using UnityEngine.InputSystem;                                  // Key
using _Project.Scripts.Galaxy.Data;                             // StarSys
using _Project.Scripts.Core.Scene;                              // SceneController, SceneId, SelectedSystemBus

namespace _Project.Scripts.SystemMap
{
    /// <summary>
    /// Тонкий оркестратор: выбирает активную StarSys и делегирует рисование слоям карты.
    /// Никакой геометрии/префабов/линий внутри — всё в слоях.
    /// </summary>
    public sealed class SystemMapRenderer : MonoBehaviour
    {
        [Header("Слои карты")]
        [SerializeField] private Transform layersRoot;           // родитель для слоёв (если пусто — создадим)
        [SerializeField] private SystemMapGeoRenderer geoLayer;  // география (звезда/планеты/орбиты)
        [SerializeField] private MonoBehaviour[] extraLayers;    // будущие слои (напр. SystemMapShipRenderer), опционально

        private Core.Core _core;                                 // доступ к вводу и галактике
        private bool _isExiting;

        private void Awake()
        {
            _core = FindFirstObjectByType<Core.Core>();
            if (!layersRoot)
            {
                var go = new GameObject("SystemMapLayers");
                go.transform.SetParent(transform, false);
                layersRoot = go.transform;
            }
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

        private async void OnEscPressed()
        {
            if (_isExiting) return;
            _isExiting = true;
            await SceneController.LoadAsync(SceneId.GalaxyMap);
        }

        private void Start()
        {
            var sys = ResolveActiveSystem();                     // выберем систему (SelectedSystemBus или [0])
            if (sys == null) { Debug.LogWarning("[SystemMap] Нет данных системы."); return; }

            // Инициализируем и рендерим геослой
            if (geoLayer != null)
            {
                geoLayer.Init(layersRoot);
                geoLayer.Render(sys.Value);
            }

            // Инициализируем и рендерим дополнительные слои (если есть)
            if (extraLayers != null)
            {
                foreach (var mb in extraLayers)
                {
                    if (mb is ISystemMapLayer layer)
                    {
                        layer.Init(layersRoot);
                        layer.Render(sys.Value);
                    }
                }
            }
        }

        private StarSys? ResolveActiveSystem()                    // выбор активной системы
        {
            if (SelectedSystemBus.HasValue) return SelectedSystemBus.Selected;

            var galaxy = Core.Core.Galaxy;
            if (galaxy == null || galaxy.Length == 0) return null;

            return galaxy[0];
        }
    }
}
