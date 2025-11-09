using _Project.Scripts.Ships;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.SystemMap
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class SystemMapShipClickInput : MonoBehaviour // обрабатывает клики по кораблям
    {
        [SerializeField] private Camera targetCamera; // камера, из которой пускаем луч

        public void Configure(Camera cam) // задать камеру извне
        {
            targetCamera = cam;
        }

        private void Awake()
        {
            if (!targetCamera)
                targetCamera = GetComponent<Camera>();
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
                return; // нечего делать

            if (!targetCamera)
                return;

            var ray = targetCamera.ScreenPointToRay(mouse.position.ReadValue()); // луч из камеры
            if (!Physics.Raycast(ray, out var hit))
                return;

            var reporter = hit.transform.GetComponentInParent<ShipClickReporter>(); // ищем компонент
            if (reporter != null)
                reporter.ReportClick(); // выводим данные
        }
    }
}

