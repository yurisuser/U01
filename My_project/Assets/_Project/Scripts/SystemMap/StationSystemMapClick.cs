using _Project.Scripts.Selection;
using _Project.Scripts.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.SystemMap
{
    /// <summary>ЛКМ по станции в системной карте.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SelectableData))]
    public sealed class StationSystemMapClick : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private ObjectInfoController objectInfoPanel;
        [SerializeField] private bool logClick = false;
        private SelectableData _data;

        private void Awake()
        {
            if (!targetCamera)
                targetCamera = Camera.main;
            if (!objectInfoPanel)
                objectInfoPanel = FindFirstObjectByType<ObjectInfoController>();
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
                objectInfoPanel?.ClearStarInfo();
                return;
            }

            objectInfoPanel?.ShowObjectInfo(_data);

            if (!logClick)
                return;

            UnityEngine.Debug.Log($"[SystemMap][Station] Click | UID={_data.Uid.Type}/{_data.Uid.Id}", this);
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
