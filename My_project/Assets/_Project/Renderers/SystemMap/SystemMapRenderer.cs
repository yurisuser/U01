using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Core.Scene;

namespace _Project.Scripts.SystemMap
{
    /// <summary>Управляет слоями рендера карты системы и обновляет их актуальными данными.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SystemMapShipRenderer))]
    public sealed class SystemMapRenderer : MonoBehaviour
    {
        [Header("Корневой объект для слоёв")]
        [SerializeField] private Transform layersRoot;
        [SerializeField] private SystemMapGeoRenderer geoLayer;
        [SerializeField] private SystemMapShipRenderer shipLayer;

        [Header("Масштаб элементов системы")]
        [SerializeField] private float starScale = 1f;
        [SerializeField] private float planetScale = 1f;
        [SerializeField] private float moonScale = 1f;
        [SerializeField] private float planetOrbitScale = 1f;
        [SerializeField] private float moonOrbitScale = 1f;

        private GameBootstrap _core;
        private GameStateService _state;
        private bool _isExiting;
        private UID _currentSystemUid;
        private int _mainThreadId;

        private void Awake()
        {
            _core = FindFirstObjectByType<GameBootstrap>();
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            if (!layersRoot)
            {
                var rootGo = new GameObject("SystemMapLayers");
                rootGo.transform.SetParent(transform, false);
                layersRoot = rootGo.transform;
            }

            if (!geoLayer)
                geoLayer = GetComponent<SystemMapGeoRenderer>() ?? GetComponentInChildren<SystemMapGeoRenderer>(true);
            if (!shipLayer)
                shipLayer = GetComponent<SystemMapShipRenderer>() ?? GetComponentInChildren<SystemMapShipRenderer>(true) ?? gameObject.AddComponent<SystemMapShipRenderer>();
        }

        private void OnEnable()
        {
            _isExiting = false;
            if (_core?.Input != null)
                _core.Input.Subscribe(Key.Escape, OnEscPressed);

            _state = GameBootstrap.GameState;
            if (_state != null)
            {
                _state.StateChanged += OnStateChanged;
                OnStateChanged();
            }
        }

        private void OnDisable()
        {
            if (_core?.Input != null)
                _core.Input.Unsubscribe(Key.Escape, OnEscPressed);

            if (_state != null)
                _state.StateChanged -= OnStateChanged;
            _state = null;
            _currentSystemUid = default;
        }

        private async void OnEscPressed()
        {
            if (_isExiting)
                return;

            _isExiting = true;
            await SceneController.LoadAsync(SceneId.GalaxyMap);
        }

        private void OnStateChanged()
        {
            if (Thread.CurrentThread.ManagedThreadId != _mainThreadId)
                return;

            var system = ResolveActiveSystem(_state);
            if (system == null)
            {
                ClearLayers();
                Debug.LogWarning("[SystemMap] Нет выбранной системы для отображения.");
                return;
            }

            bool systemChanged = !_currentSystemUid.Equals(system.Value.Uid);
            if (systemChanged)
            {
                ClearLayers();
                _currentSystemUid = system.Value.Uid;
            }

            RenderStaticSystem(system.Value, systemChanged);
            RenderShips(system.Value, systemChanged);
        }

        private void Update()
        {
            if (_state == null)
                return;

            var system = ResolveActiveSystem(_state);
            if (system == null)
                return;

            RenderShips(system.Value, false);
        }

        private void RenderStaticSystem(in StarSys system, bool systemChanged)
        {
            if (geoLayer != null)
            {
                geoLayer.SetScaleOverrides(
                    Mathf.Max(0.0001f, starScale),
                    Mathf.Max(0.0001f, planetScale),
                    Mathf.Max(0.0001f, moonScale),
                    Mathf.Max(0.0001f, planetOrbitScale),
                    Mathf.Max(0.0001f, moonOrbitScale));

                if (systemChanged)
                    geoLayer.Init(layersRoot);
                geoLayer.Render(system);
            }

            if (systemChanged)
                shipLayer?.Init(layersRoot);
        }

        private void RenderShips(in StarSys system, bool systemChanged)
        {
            if (!shipLayer)
                return;

            if (systemChanged)
                shipLayer.Init(layersRoot);

            shipLayer.Render(system);
        }

        private void ClearLayers()
        {
            geoLayer?.Dispose();
            _currentSystemUid = default;
            shipLayer?.Dispose();
        }

        private static StarSys? ResolveActiveSystem(GameStateService state)
        {
            var galaxy = state?.Galaxy;
            if (galaxy == null || galaxy.Length == 0)
                return null;

            var index = state.SelectedSystemIndex;
            if (index >= 0 && index < galaxy.Length)
                return galaxy[index];

            return galaxy[0];
        }

    }
}
