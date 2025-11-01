using UnityEngine;
using UnityEngine.InputSystem;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Core.Scene;
using _Project.Scripts.Ships;

namespace _Project.Scripts.SystemMap
{
    /// <summary>
    /// Управляет набором слоёв на экране системы и передаёт им снимки данных.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SystemMapRenderer : MonoBehaviour
    {
        [Header("Корневой объект для слоёв")]
        [SerializeField] private Transform layersRoot;           // сюда вешаем все созданные GO
        [SerializeField] private SystemMapGeoRenderer geoLayer;  // слой геометрии (звезда, орбиты, планеты)
        [SerializeField] private MonoBehaviour[] extraLayers;    // дополнительные слои (корабли и т.п.)

        private GameBootstrap _core;
        private GameStateService _state;
        private bool _isExiting;

        private void Awake()
        {
            _core = FindFirstObjectByType<GameBootstrap>();
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
                OnRenderChanged(_state.Render); // сразу показываем актуальные данные
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
            var system = ResolveActiveSystem(snapshot);
            if (system == null)
            {
                ClearLayers();
                Debug.LogWarning("[SystemMap] Нет выбранной системы для отображения.");
                return;
            }

            RenderSystem(system.Value, snapshot.Ships, snapshot.ShipCount);
        }

        private void RenderSystem(in StarSys system, Ship[] ships, int shipCount)
        {
            ClearLayers();

            if (geoLayer != null)
            {
                geoLayer.Init(layersRoot);
                geoLayer.Render(system, ships, shipCount);
            }

            if (extraLayers == null)
                return;

            for (int i = 0; i < extraLayers.Length; i++)
            {
                if (extraLayers[i] is ISystemMapLayer layer)
                {
                    layer.Init(layersRoot);
                    layer.Render(system, ships, shipCount);
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
