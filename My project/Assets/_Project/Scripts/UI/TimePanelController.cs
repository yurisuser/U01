using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.UI
{
    public class TimePanelController : MonoBehaviour
    {
        private Toggle _playToggle;

        // Храним ссылку на колбэк, чтобы корректно отписаться
        private EventCallback<ChangeEvent<bool>> _onToggleChanged;

        private void OnEnable()
        {
            var doc = GetComponent<UIDocument>();
            if (doc == null)
            {
                Debug.LogError("[TimePanel] UIDocument not found on GameObject.");
                return;
            }

            var root = doc.rootVisualElement;
            _playToggle = root.Q<Toggle>("PlayToggle");
            if (_playToggle == null)
            {
                Debug.LogError("[TimePanel] Toggle 'PlayToggle' not found. Check UXML name.");
                return;
            }

            // Создаем делегат один раз и сохраняем
            _onToggleChanged = OnPlayToggleChanged;

            // На всякий случай — снимаем, если кто-то уже повесил раньше
            _playToggle.UnregisterValueChangedCallback(_onToggleChanged);
            _playToggle.RegisterValueChangedCallback(_onToggleChanged);

            Debug.Log("[TimePanel] Ready. Value = " + _playToggle.value);
        }

        private void OnDisable()
        {
            if (_playToggle != null && _onToggleChanged != null)
            {
                _playToggle.UnregisterValueChangedCallback(_onToggleChanged);
            }
        }

        private void OnPlayToggleChanged(ChangeEvent<bool> evt)
        {
            Debug.Log("[TimePanel] Toggled. Value = " + evt.newValue);
            // тут дергай свой GameState: Paused/Running
            // GameState.RunMode = evt.newValue ? RunMode.Running : RunMode.Paused;
        }
    }
}