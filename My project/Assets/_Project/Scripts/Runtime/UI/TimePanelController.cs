using _Project.Scripts.Core;                  // GameBootstrap
using _Project.Scripts.Core.GameState;        // ERunMode
using UnityEngine;                            // MonoBehaviour, Debug
using UnityEngine.UIElements;                 // UI Toolkit: UIDocument, Toggle, callbacks


namespace _Project.Scripts.UI
{
    public class TimePanelController : MonoBehaviour
    {
        private static TimePanelController _instance;

        private Toggle _playToggle;                                   // Toggle из UXML (name="PlayPauseToggle")
        private EventCallback<ChangeEvent<bool>> _onToggleChanged;    // Делегат, чтобы можно было отписаться
        private bool _isPlaying = false;                              // Текущее состояние UI: true=Auto, false=Paused

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            var doc = GetComponent<UIDocument>();                     // Документ UI Toolkit на этом GO
            if (doc == null) { Debug.LogError("[TimePanel] UIDocument not found."); return; }

            var root = doc.rootVisualElement;                         // Корневой UXML
            _playToggle = root.Q<Toggle>("PlayPauseToggle");          // Ищем Toggle по name=PlayPauseToggle

            if (_playToggle == null && doc.visualTreeAsset != null)   // fallback: иногда документ ещё пуст - клонируем вручную
            {
                root.Clear();
                doc.visualTreeAsset.CloneTree(root);
                _playToggle = root.Q<Toggle>("PlayPauseToggle");
            }

            if (_playToggle == null)
            {
                Debug.LogError("[TimePanel] Toggle 'PlayPauseToggle' not found.");
                return;
            }

            _onToggleChanged = OnPlayToggleChanged;                   // Подготовили делегат для подписки
            _playToggle.UnregisterValueChangedCallback(_onToggleChanged); // Чистим защитно – вдруг кто-то уже подписал
            _playToggle.RegisterValueChangedCallback(_onToggleChanged);   // Подписываемся у UI Toolkit

            _isPlaying = _playToggle.value;                           // Считываем начальное состояние
            GameBootstrap.GameState.SetRunMode(_isPlaying ? ERunMode.Auto : ERunMode.Paused); // Отражаем статус в ядре
        }

        private void OnDisable()
        {
            if (_playToggle != null && _onToggleChanged != null)
                _playToggle.UnregisterValueChangedCallback(_onToggleChanged); // снимаем подписку
        }

        private void OnPlayToggleChanged(ChangeEvent<bool> evt)        // Обработка клика по Play/Pause
        {
            _isPlaying = evt.newValue;                                 // Обновляем локальный флаг
            GameBootstrap.GameState.SetRunMode(_isPlaying ? ERunMode.Auto : ERunMode.Paused); // переключаем режим симуляции
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
