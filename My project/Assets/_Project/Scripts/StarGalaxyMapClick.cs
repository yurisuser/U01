using UnityEngine;
using UnityEngine.InputSystem;   // новый Input System
using _Project.Galaxy.Obj;      // StarType

namespace _Project.Scripts
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class StarGalaxyMapClick : MonoBehaviour
    {
        [Header("Данные (заполняй из рендерера/префаба)")]
        public StarType Type;
        public string SystemName;

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
            // Мышь
            if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
                TryClick(Mouse.current.position.ReadValue());

            // Тач (ВАЖНО: у TouchControl нет wasReleasedThisFrame — читаем через press)
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

            // засчитываем только попадание по нашему объекту/его детям
            if (hit.collider == _col || hit.collider.transform.IsChildOf(transform))
                Debug.Log($"[Star] {SystemName} → {Type}");
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