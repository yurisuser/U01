using _Project.Scripts.Galaxy.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using _Project.Scripts.Core.Scene;

namespace _Project.Scripts.GalaxyMap.Runtime
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class StarGalaxyMapClick : MonoBehaviour
    {
        [SerializeField] private bool   logClick = true;

        [Header("Данные (заполняет рендерер/префаб)")]
        public EStarType type;
        public string   systemName;
        public StarSys? System;

        [Header("Камера (если пусто — возьмёт MainCamera)")]
        [SerializeField] private Camera cam;

        private Collider _col;

        private void Awake()
        {
            _col = GetComponent<Collider>();
            if (!cam) cam = Camera.main;
        }

        private void Update()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
                TryClick(Mouse.current.position.ReadValue());

            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.press.wasReleasedThisFrame)
                    TryClick(touch.position.ReadValue());
            }
        }

        private void TryClick(Vector2 screenPos)
        {
            var c = cam ? cam : Camera.main;
            if (!c) return;

            var ray = c.ScreenPointToRay(screenPos);
            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, ~0)) return;

            if (hit.collider == _col || hit.collider.transform.IsChildOf(transform))
            {
                if (logClick) Debug.Log($"[Star] {systemName} → {type}");

                if (System.HasValue)
                    { 
                        SelectedSystemBus.Selected = System.Value;
                        SelectedSystemBus.HasValue = true;
                    }
                SceneController.Load(SceneId.SystemMap);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!cam) cam = Camera.main;
            _col = GetComponent<Collider>();
        }
#endif
    }
}


