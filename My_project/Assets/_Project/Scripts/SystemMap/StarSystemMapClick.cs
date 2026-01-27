using _Project.Scripts.Core;
using _Project.Scripts.Selection;
using _Project.Scripts.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.SystemMap
{
    /// <summary>ЛКМ по звезде в системной карте.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SelectableData))]
    public sealed class StarSystemMapClick : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private ObjectInfoController starInfoPanel;
        [SerializeField] private bool logClick = false;
        private SelectableData _data;

        private void Awake()
        {
            if (!targetCamera)
                targetCamera = Camera.main;
            if (!starInfoPanel)
                starInfoPanel = FindFirstObjectByType<ObjectInfoController>();
            _data = GetComponent<SelectableData>();
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
            if (_data == null || !_data.HasData)
            {
                starInfoPanel?.ClearStarInfo();
                return;
            }

            starInfoPanel?.ShowObjectInfo(_data);

            if (!logClick)
                return;

            UnityEngine.Debug.Log($"[SystemMap][Star] Click | UID={_data.Uid.Type}/{_data.Uid.Id}", this);
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
