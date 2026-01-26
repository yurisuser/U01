using _Project.Scripts.Core;
using _Project.Scripts.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.SystemMap
{
    /// <summary>ЛКМ по звезде в системной карте.</summary>
    [DisallowMultipleComponent]
    public sealed class StarSystemMapClick : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private GalaxyMapStarInfoController starInfoPanel;
        [SerializeField] private bool logClick = false;

        private void Awake()
        {
            if (!targetCamera)
                targetCamera = Camera.main;
            if (!starInfoPanel)
                starInfoPanel = FindFirstObjectByType<GalaxyMapStarInfoController>();
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
                return;

            if (!targetCamera)
                return;

            var ray = targetCamera.ScreenPointToRay(mouse.position.ReadValue());
            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, ~0))
                return;

            if (!hit.transform || !hit.transform.IsChildOf(transform))
                return;

            OnLeftClick();
        }

        private void OnLeftClick()
        {
            var sys = GameBootstrap.GameState.GetSelectedSystem();
            if (sys.HasValue)
                starInfoPanel?.ShowStarInfo(sys.Value.Uid);
            else
                starInfoPanel?.ClearStarInfo();

            if (!logClick)
                return;

            int id = sys.HasValue ? sys.Value.ConstellationId : -1;
            UnityEngine.Debug.Log($"[SystemMap][Star] Click | ConstellationId={id}", this);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!targetCamera)
                targetCamera = Camera.main;
        }
#endif
    }
}
