using UnityEngine;
using UnityEngine.InputSystem;

// новый Input System

namespace _Project.Scripts.SystemMap
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class SystemMapCameraController : MonoBehaviour
    {
        [SerializeField] private float minOrtho = 2f;
        [SerializeField] private float maxOrtho = 100f;
        [SerializeField] private float zoomSpeed = 0.2f;
        [SerializeField] private float panSpeed = 0.7f;

        Camera _cam;

        void Awake()
        {
            _cam = GetComponent<Camera>();
            _cam.orthographic = true;
            if (_cam.transform.position.z > -0.1f)
                _cam.transform.position = new Vector3(0, 0, -10f);
        }

        public void Frame(float maxRadius)
        {
            // немного отступа от края
            float target = Mathf.Clamp(maxRadius * 1.2f, minOrtho, maxOrtho);
            _cam.orthographicSize = target;
            _cam.transform.position = new Vector3(0, 0, _cam.transform.position.z);
        }

        void Update()
        {
            // Зум колёсиком
            if (Mouse.current != null)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    var size = _cam.orthographicSize * Mathf.Exp(-scroll * zoomSpeed * Time.unscaledDeltaTime);
                    _cam.orthographicSize = Mathf.Clamp(size, minOrtho, maxOrtho);
                }

                // Панорамирование правой кнопкой (или средней)
                bool dragging = Mouse.current.rightButton.isPressed || Mouse.current.middleButton.isPressed;
                if (dragging)
                {
                    Vector2 d = Mouse.current.delta.ReadValue(); // пиксели за кадр
                    // нормализуем движение относительно масштаба и DPI (~1000 подобрано эмпирически)
                    float k = _cam.orthographicSize / 1000f;
                    var move = new Vector3(-d.x * k * panSpeed, -d.y * k * panSpeed, 0f);
                    _cam.transform.position += move;
                }
            }
        }
    }
}
