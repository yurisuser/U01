using UnityEngine;
using UnityEngine.InputSystem;

// New Input System

namespace _Project.Scripts.GalaxyMap.Runtime
{
    [RequireComponent(typeof(Camera))]
    public class GalaxyCamera2D : MonoBehaviour
    {
        [Header("Pan bounds")]
        public float minX = -500f, maxX = 500f;
        public float minY = -500f, maxY = 500f;

        [Header("Zoom")]
        public float minZoom = 10f;
        public float maxZoom = 200f;
        public float zoomSpeed = 20f;

        [Header("Drag")]
        public float dragDamp = 1.0f;

        Camera _cam;
        Vector3 _dragStartWorld;
        bool _dragging;

        void Awake()
        {
            _cam = GetComponent<Camera>();
            _cam.orthographic = true;
        }

        void Update()
        {
            if (Mouse.current == null) return;

            // --- ЗУМ колёсиком вокруг курсора ---
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                Vector3 mouseWorldBefore = ScreenToWorld(Mouse.current.position.ReadValue());

                // ЛИНЕЙНЫЙ зум — без deltaTime и без экспоненты
                _cam.orthographicSize = Mathf.Clamp(
                    _cam.orthographicSize - scroll * zoomSpeed,  // <-- zoomSpeed ~ 5..50
                    minZoom, maxZoom
                );

                Vector3 mouseWorldAfter = ScreenToWorld(Mouse.current.position.ReadValue());
                Vector3 delta = mouseWorldBefore - mouseWorldAfter;
                transform.position = ClampPos(transform.position + delta);
            }

            // --- DRAG левой кнопкой ---
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _dragging = true;
                _dragStartWorld = ScreenToWorld(Mouse.current.position.ReadValue());
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                _dragging = false;
            }

            if (_dragging)
            {
                Vector3 curWorld = ScreenToWorld(Mouse.current.position.ReadValue());
                Vector3 delta = (_dragStartWorld - curWorld) * dragDamp;
                transform.position = ClampPos(transform.position + delta);
            }
        }

        Vector3 ScreenToWorld(Vector2 screenPos)
        {
            var p = _cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, _cam.nearClipPlane));

            // для орто-камеры z нам не важен, нормализуем:
            p.z = transform.position.z;
            return p;
        }

        Vector3 ClampPos(Vector3 pos)
        {
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            return pos;
        }
    }
}
