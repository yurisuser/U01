using System.Collections;                                         // корутина загрузки сцены
using _Project.Scripts.Core.Input;                                // контроллер ввода
using _Project.Scripts.Core.Scene;                                // менеджер сцен
using _Project.Scripts.Galaxy.Data;                               // тип StarSys
using _Project.Scripts.Galaxy.Generation;                         // генератор галактики
using UnityEngine;                                                // Unity API
using _Project.Scripts.Core.GameState;                            // GameStateService / ERunMode
using _Project.Scripts.Core.Simulation;                           // SimulationStepController (без MonoBehaviour)

namespace _Project.Scripts.Core
{
    public sealed class Core : MonoBehaviour
    {
        private static GameStateService _gameState;               // единственный экземпляр состояния игры (ленивая инициализация)
        public  static GameStateService GameState => _gameState ??= new GameStateService(2.0f); // доступ к состоянию; шаг логики = 2.0с

        public static Core Instance { get; private set; }         // синглтон ядра
        public SceneController Scenes { get; } = new SceneController(); // управление сценами
        public static StarSys[] Galaxy { get; private set; }      // сгенерированная галактика
        public InputController  Input  { get; } = new InputController(); // опрос ввода (polling)

        private readonly SimulationStepController _simulation = new SimulationStepController(); // логический тикер по таймеру (без компонентов)

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; } // защита от дублей ядра
            Instance = this;

            DontDestroyOnLoad(gameObject);                           // ядро живёт между сценами

            Galaxy = GalaxyCreator.Create();                         // создаём данные галактики
            UnityEngine.Debug.Log($"Core: Galaxy generated. Stars = {Galaxy?.Length}"); // печатаем количество звёзд

            StartCoroutine(LoadMainMenuDelayed());                   // мягкая загрузка первой сцены
        }

        private void Update()
        {
            Input?.Update();                                         // опрос ввода
            _simulation.UpdateStep(Time.deltaTime);                  // тикаем симуляцию по таймеру (один шаг при накоплении)
        }

        private IEnumerator LoadMainMenuDelayed()                    // небольшая задержка для корректной инициализации
        {
            yield return new WaitForSeconds(1f);
            SceneController.Load(SceneId.GalaxyMap);                 // статический вызов загрузки сцены
        }
    }
}
