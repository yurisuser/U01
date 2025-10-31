using UnityEngine;                                              // MonoBehaviour, Transform
using UnityEngine.InputSystem;                                  // Key
using _Project.Scripts.Galaxy.Data;                             // StarSys
using _Project.Scripts.Core;                                    // GameBootstrap
using _Project.Scripts.Core.GameState;                          // GameStateService
using _Project.Scripts.Core.Scene;                              // SceneController, SceneId

namespace _Project.Scripts.SystemMap
{
    /// <summary>
    /// ������ �ථ�����: �롨ࠥ� ��⨢��� StarSys � ��������� �ᮢ���� ᫮� �����.
    /// ������� ������ਨ/��䠡��/����� ����� - ��� � ᫮��.
    /// </summary>
    public sealed class SystemMapRenderer : MonoBehaviour
    {
        [Header("���� �����")]
        [SerializeField] private Transform layersRoot;           // த�⥫� ��� ᫮� (�᫨ ���� - ᮧ�����)
        [SerializeField] private SystemMapGeoRenderer geoLayer;  // ������� (������/�������/�ࡨ��)
        [SerializeField] private MonoBehaviour[] extraLayers;    // ���騥 ᫮� (����. SystemMapShipRenderer), ��樮���쭮

        private GameBootstrap    _core;                          // ����� � ����� � �����⨪�
        private GameStateService _state;                         // ������ ���ﭨ�
        private bool             _isExiting;

        private void Awake()
        {
            _core = FindFirstObjectByType<GameBootstrap>();
            if (!layersRoot)
            {
                var go = new GameObject("SystemMapLayers");
                go.transform.SetParent(transform, false);
                layersRoot = go.transform;
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
                OnRenderChanged(_state.Render); // ��� ����� �� ������ ����������
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
            if (_isExiting) return;
            _isExiting = true;
            await SceneController.LoadAsync(SceneId.GalaxyMap);
        }

        private void OnRenderChanged(GameStateService.RenderSnapshot snapshot)
        {
            var sys = ResolveActiveSystem(snapshot);
            if (sys == null)
            {
                ClearLayers();
                Debug.LogWarning("[SystemMap] ��� ������ ��⥬�.");
                return;
            }

            RenderSystem(sys.Value);
        }

        private void RenderSystem(in StarSys sys)
        {
            ClearLayers();

            if (geoLayer != null)
            {
                geoLayer.Init(layersRoot);
                geoLayer.Render(sys);
            }

            if (extraLayers == null) return;
            for (int i = 0; i < extraLayers.Length; i++)
            {
                if (extraLayers[i] is ISystemMapLayer layer)
                {
                    layer.Init(layersRoot);
                    layer.Render(sys);
                }
            }
        }

        private void ClearLayers()
        {
            geoLayer?.Dispose();

            if (extraLayers == null) return;
            for (int i = 0; i < extraLayers.Length; i++)
            {
                if (extraLayers[i] is ISystemMapLayer layer)
                    layer.Dispose();
            }
        }

        private static StarSys? ResolveActiveSystem(GameStateService.RenderSnapshot snapshot) // �롮� ��⨢��� ��⥬�
        {
            var galaxy = snapshot.Galaxy;
            if (galaxy == null || galaxy.Length == 0) return null;

            var idx = snapshot.SelectedSystemIndex;
            if (idx >= 0 && idx < galaxy.Length)
                return galaxy[idx];

            return galaxy[0];
        }
    }
}
