using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Core.Scene;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.GalaxyMap.Runtime
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class StarGalaxyMapClick : MonoBehaviour
    {
        [SerializeField] private bool   logClick = true;

        [Header("����� (�������� ७����/��䠡)")]
        public EStarType type;
        public string   systemName;
        public StarSys? System;

        [Header("����� (�᫨ ���� - ������ MainCamera)")]
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
                if (logClick) Debug.Log($"[Star] {systemName}  {type}");

                if (System.HasValue)
                {
                    var sys = System.Value;
                    if (!GameBootstrap.GameState.SelectSystemByUid(sys.Uid))
                        GameBootstrap.GameState.SelectSystemByIndex(0);
                }
                else
                {
                    GameBootstrap.GameState.ClearSelectedSystem();
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
