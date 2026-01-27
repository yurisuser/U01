using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Core.Scene;
using _Project.Scripts.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.GalaxyMap.Runtime
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class StarGalaxyMapClick : MonoBehaviour
    {
        [SerializeField] private bool   logClick = true;

        public EStarType type;
        public string   systemName;
        public StarSys? System;

        [SerializeField] private Camera cam;

        [Header("UI (панель звезды)")]
        [SerializeField] private ObjectInfoController starInfoPanel;
        [SerializeField] private float doubleClickThreshold = 0.25f;

        private Collider _col;
        private float _lastLeftClickTime = -1f;
        private Coroutine _singleClickRoutine;

        private void Awake()
        {
            _col = GetComponent<Collider>();
            if (!cam) cam = Camera.main;
            if (!starInfoPanel)
                starInfoPanel = FindFirstObjectByType<ObjectInfoController>();
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
            float timeSince = Time.time - _lastLeftClickTime;
            if (timeSince <= doubleClickThreshold)
            {
                _lastLeftClickTime = -1f;
                if (_singleClickRoutine != null)
                {
                    StopCoroutine(_singleClickRoutine);
                    _singleClickRoutine = null;
                }
                OnDoubleLeftClick();
                return;
            }

            _lastLeftClickTime = Time.time;
            if (_singleClickRoutine != null)
                StopCoroutine(_singleClickRoutine);
            _singleClickRoutine = StartCoroutine(DelayedSingleClick());
        }

        private void OnRightClick()
        {
            if (System.HasValue)
                starInfoPanel?.ShowStarInfo(System.Value.Uid);
            else
                starInfoPanel?.ClearStarInfo();

            if (!logClick)
                return;

            int constellationId = System.HasValue ? System.Value.ConstellationId : -1;
            Debug.Log($"[Star][ПКМ] {systemName} | ConstellationId={constellationId}");
        }

        private IEnumerator DelayedSingleClick()
        {
            yield return new WaitForSeconds(doubleClickThreshold);
            _singleClickRoutine = null;
            if (_lastLeftClickTime < 0f)
                yield break;

            OnSingleLeftClick();
        }

        private void OnSingleLeftClick()
        {
            if (System.HasValue)
                starInfoPanel?.ShowStarInfo(System.Value.Uid);
            else
                starInfoPanel?.ClearStarInfo();
        }

        private void OnDoubleLeftClick()
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

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!cam) cam = Camera.main;
            _col = GetComponent<Collider>();
        }
#endif
    }
}
