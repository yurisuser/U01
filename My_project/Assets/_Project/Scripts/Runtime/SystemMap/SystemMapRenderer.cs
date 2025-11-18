using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Core.Scene;
using _Project.Scripts.Ships;
using sim = _Project.Scripts.Simulation;

namespace _Project.Scripts.SystemMap
{
    /// <summary>
    /// Управляет слоями рендера карты системы, подсовывает им актуальные данные.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SystemMapRenderer : MonoBehaviour
    {
        [Header("Корневой объект для слоёв")]
        [SerializeField] private Transform layersRoot;
        [SerializeField] private SystemMapGeoRenderer geoLayer;
        [SerializeField] private MonoBehaviour[] extraLayers;

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
        }

        private void OnEnable()
        {
            _isExiting = false;
            if (_core?.Input != null)
                _core.Input.Subscribe(Key.Escape, OnEscPressed);

            _state = GameBootstrap.GameState;
            if (_state != null)
            {
                _state.RenderChanged += OnRenderChanged;
                OnRenderChanged(_state.Render);
            }
        }

        private void OnDisable()
        {
            if (_core?.Input != null)
                _core.Input.Unsubscribe(Key.Escape, OnEscPressed);

            if (_state != null)
                _state.RenderChanged -= OnRenderChanged;
            _state = null;
        }

        private async void OnEscPressed()
        {
            if (_isExiting)
                return;

            _isExiting = true;
            await SceneController.LoadAsync(SceneId.GalaxyMap);
        }

        private void OnRenderChanged(GameStateService.RenderSnapshot snapshot)
        {
            if (Thread.CurrentThread.ManagedThreadId != _mainThreadId)
                return;

            var system = ResolveActiveSystem(snapshot);
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

            RenderSystem(system.Value,
                snapshot.PreviousShips,
                snapshot.PreviousShipCount,
                snapshot.CurrentShips,
                snapshot.CurrentShipCount,
                snapshot.NextShips,
                snapshot.NextShipCount,
                snapshot.StepProgress,
                Mathf.Max(0.0001f, snapshot.LogicStepSeconds),
                systemChanged,
                snapshot.Substeps,
                snapshot.SubstepsVersion);
        }

        private void RenderSystem(
            in StarSys system,
            Ship[] prevShips,
            int prevCount,
            Ship[] currShips,
            int currCount,
            Ship[] nextShips,
            int nextCount,
            float progress,
            float stepDuration,
            bool systemChanged,
            System.Collections.Generic.IReadOnlyDictionary<UID, System.Collections.Generic.List<sim.Render.SubstepSample>> substeps,
            int substepsVersion)
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
                geoLayer.Render(system, prevShips, prevCount, currShips, currCount, nextShips, nextCount, progress, stepDuration, substeps);
            }

            if (extraLayers == null)
                return;

            for (int i = 0; i < extraLayers.Length; i++)
            {
                if (extraLayers[i] is ISystemMapLayer layer)
                {
                    if (systemChanged)
                        layer.Init(layersRoot);
                    layer.Render(system, prevShips, prevCount, currShips, currCount, nextShips, nextCount, progress, stepDuration, substeps);
                }
            }
        }

        private void ClearLayers()
        {
            geoLayer?.Dispose();

            if (extraLayers == null)
                return;

            for (int i = 0; i < extraLayers.Length; i++)
            {
                if (extraLayers[i] is ISystemMapLayer layer)
                    layer.Dispose();
            }
        }

        private static StarSys? ResolveActiveSystem(GameStateService.RenderSnapshot snapshot)
        {
            var galaxy = snapshot.Galaxy;
            if (galaxy == null || galaxy.Length == 0)
                return null;

            var index = snapshot.SelectedSystemIndex;
            if (index >= 0 && index < galaxy.Length)
                return galaxy[index];

            return galaxy[0];
        }
    }
}
