using System.Collections;                                         // корутина загрузки сцены
using _Project.Scripts.Core.Input;                                // контроллер ввода
using _Project.Scripts.Core.Scene;                                // менеджер сцен
using _Project.Scripts.Galaxy.Data;                               // тип StarSys
using _Project.Scripts.Galaxy.Generation;                         // генератор галактики
using UnityEngine;                                                // Unity API
using _Project.Scripts.Core.GameState;

namespace _Project.Scripts.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        private static GameStateService _gameState;               
        public  static GameStateService GameState => _gameState ??= new GameStateService(2.0f); // доступ к состоянию; шаг логики = 2.0с
        public static GameBootstrap Instance { get; private set; }         // синглтон ядра
        public SceneController Scenes { get; } = new SceneController(); // управление сценами
        public static StarSys[] Galaxy { get; private set; }      // сгенерированная галактика
        public InputController  Input  { get; } = new InputController(); // опрос ввода (polling)
        [SerializeField] private float stepDurationSeconds = 2f; // длительность логического шага
        private StepManager _stepManager;                          // тупой диспетчер шага

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            DontDestroyOnLoad(gameObject);
            Galaxy = GalaxyCreator.Create();                         // создаём данные галактики
            _stepManager = new StepManager(stepDurationSeconds, (_, __) => { });

            StartCoroutine(LoadMainMenuDelayed());                   // мягкая загрузка первой сцены
        }

        private void Update()
        {
            Input?.Update();                                         // опрос ввода
            _stepManager?.Update(Time.deltaTime);
        }

        private IEnumerator LoadMainMenuDelayed()                    // небольшая задержка для корректной инициализации
        {
            yield return new WaitForSeconds(1f);
            SceneController.Load(SceneId.GalaxyMap);                 // статический вызов загрузки сцены
        }
    }
}
