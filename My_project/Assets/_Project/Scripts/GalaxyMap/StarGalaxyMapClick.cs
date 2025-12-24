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
            var mouse = Mouse.current;
            if (mouse == null)
                return;

            bool left = mouse.leftButton.wasPressedThisFrame;
            bool right = mouse.rightButton.wasPressedThisFrame;
            if (!left && !right)
                return;

            if (!cam)
                return;

            var ray = cam.ScreenPointToRay(mouse.position.ReadValue());
            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, ~0))
                return;

            if (hit.collider != _col && !hit.collider.transform.IsChildOf(transform))
                return;

            if (left)
                OnLeftClick();
            else if (right)
                OnRightClick();
        }

        private void OnLeftClick()
        {
            if (logClick)
            {
                int constellationId = System.HasValue ? System.Value.ConstellationId : -1;
                Debug.Log($"[Star] {systemName} | ConstellationId={constellationId}");
            }

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

        private void OnRightClick()
        {
            if (!logClick)
                return;

            int constellationId = System.HasValue ? System.Value.ConstellationId : -1;
            Debug.Log($"[Star][ПКМ] {systemName} | ConstellationId={constellationId}");
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
