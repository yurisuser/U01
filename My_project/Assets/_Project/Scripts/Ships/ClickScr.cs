using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState.GameStateMembers.SelectedObj;
using _Project.Scripts.Selection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Ships
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SelectableData))]
    public sealed class ClickScr : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        private SelectableData _data;

        private void Awake()
        {
            if (!targetCamera)
                targetCamera = Camera.main;
            _data = GetComponent<SelectableData>();
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null)
                return;

            bool left = mouse.leftButton.wasPressedThisFrame;
            bool right = mouse.rightButton.wasPressedThisFrame;
            if (!left && !right)
                return;

            if (!targetCamera)
                return;

            var ray = targetCamera.ScreenPointToRay(mouse.position.ReadValue());
            if (!Physics.Raycast(ray, out var hit))
                return;

            if (!hit.transform || !hit.transform.IsChildOf(transform))
                return;

            if (left)
                OnLeftClick();
            else if (right)
                OnRightClick();
        }

        private void OnLeftClick()
        {
            Debug.Log("Клик ЛКМ по кораблю", this);
            SetSelected();
        }

        private void OnRightClick()
        {
            Debug.Log("Клик ПКМ по кораблю", this);
        }

        private void SetSelected()
        {
            if (_data == null || !_data.HasData)
            {
                Debug.LogWarning("Клик по кораблю, но не заданы данные (UID/система).", this);
                return;
            }

            int systemIndex = _data.SystemIndex >= 0 ? _data.SystemIndex : GameBootstrap.GameState.SelectedSystemIndex;
            GameBootstrap.GameState.SelectedService.SetSelected(systemIndex, _data.Uid, _data.SelectedType);
        }
    }
}
