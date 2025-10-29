using _Project.Scripts.Core;                  // GameBootstrap
using _Project.Scripts.Core.GameState;        // ERunMode
using UnityEngine;                            // MonoBehaviour, Debug
using UnityEngine.UIElements;                 // UI Toolkit: UIDocument, Toggle, callbacks


namespace _Project.Scripts.UI
{
    public class TimePanelController : MonoBehaviour
    {
        private Toggle _playToggle;                                   // Toggle из UXML (name="PlayPauseToggle")
        private EventCallback<ChangeEvent<bool>> _onToggleChanged;    // сохранённый делегат для корректного un-register
        private bool _isPlaying = false;                              // локальное зеркало UI: true=Auto, false=Paused

        private void OnEnable()
        {
            var doc = GetComponent<UIDocument>();                     // корневой документ UI Toolkit на этом GO
            if (doc == null) { Debug.LogError("[TimePanel] UIDocument not found."); return; }

            var root = doc.rootVisualElement;                         // корень UXML
            _playToggle = root.Q<Toggle>("PlayPauseToggle");          // ищем Toggle по name=PlayPauseToggle
            if (_playToggle == null) { Debug.LogError("[TimePanel] Toggle 'PlayPauseToggle' not found."); return; }

            _onToggleChanged = OnPlayToggleChanged;                   // создаём делегат один раз
            _playToggle.UnregisterValueChangedCallback(_onToggleChanged); // страховка от двойных подписок
            _playToggle.RegisterValueChangedCallback(_onToggleChanged);   // подписываемся по UI Toolkit

            _isPlaying = _playToggle.value;                           // синхронизация начального состояния
            GameBootstrap.GameState.SetRunMode(_isPlaying ? ERunMode.Auto : ERunMode.Paused); // инициализируем режим
        }

        private void OnDisable()
        {
            if (_playToggle != null && _onToggleChanged != null)
                _playToggle.UnregisterValueChangedCallback(_onToggleChanged); // снятие подписки
        }

        private void OnPlayToggleChanged(ChangeEvent<bool> evt)        // единая точка обработки Play/Pause
        {
            _isPlaying = evt.newValue;                                 // обновляем локальное зеркало
            GameBootstrap.GameState.SetRunMode(_isPlaying ? ERunMode.Auto : ERunMode.Paused); // переключаем режим симуляции
            
            Debug.Log("[TimePanel] Play=" + _isPlaying);               // диагностика
        }
    }
}
