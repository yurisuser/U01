using System.Collections;
using System.IO;
using _Project.DataAccess;             // для каталога и прогрева БД
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Core.Input;
using _Project.Scripts.Core.Scene;
using _Project.Scripts.Galaxy.Generation;
using _Project.Scripts.Simulation.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Core
{
    /// <summary>Точка входа игры: создаёт контекст, стейт и запускает симуляцию.</summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        private static GameStateService _gameState;
        private SimulationRootController _simulation;
        private SimulationClock _simulationClock;

        /// <summary>Глобальный сервис состояния игры.</summary>
        public static GameStateService GameState
        {
            get
            {
                if (_gameState == null)
                    _gameState = new GameStateService();

                return _gameState;
            }
        }

        /// <summary>Синглтон экземпляра GameBootstrap.</summary>
        public static GameBootstrap Instance { get; private set; }

        /// <summary>Менеджер сцен.</summary>
        public SceneController Scenes { get; } = new SceneController();
        /// <summary>Контроллер ввода.</summary>
        public InputController Input { get; } = new InputController();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (_gameState == null)
                _gameState = new GameStateService();

            CatalogPreloader.PreloadAll(forceReload: true); // прогреваем все каталоги в память
            StarNameCatalog.Initialize();

            var galaxy = GalaxyCreator.Create();
            _gameState.SetGalaxy(galaxy);
            _gameState.DeactivateLocalSystem(); // До входа в SystemMap локальная симуляция всегда отключена.

            // Стартуем минимальный каркас симуляции: пока заглушки.
            _simulationClock = new SimulationClock(Time.fixedDeltaTime);
            _simulation = new SimulationRootController(_gameState, _simulationClock);
            SceneManager.sceneLoaded += OnSceneLoaded;

            StartCoroutine(LoadMainMenuDelayed());
        }

        private void Update()
        {
            Input?.Update();
        }

        private void FixedUpdate()
        {
            _simulation?.TickFixed(Time.fixedDeltaTime);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _simulation?.Dispose();
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            if (_gameState == null)
                return;

            if (scene.name == "SystemMap")
            {
                _gameState.ActivateLocalFromSelectedSystem(); // Локал активен только в сцене системы.
                return;
            }

            _gameState.DeactivateLocalSystem(); // Во всех прочих сценах локал не тикается.
        }

        private IEnumerator LoadMainMenuDelayed()
        {
            yield return new WaitForSeconds(1f);
            SceneController.Load(SceneId.GalaxyMap);
        }
    }
}
