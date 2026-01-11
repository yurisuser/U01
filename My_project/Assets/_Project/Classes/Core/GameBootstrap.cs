using System.Collections;
using System.IO;
using _Project.DataAccess;             // для каталога и прогрева БД
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Core.Input;
using _Project.Scripts.Core.Scene;
using _Project.Scripts.Galaxy.Generation;
using _Project.Scripts.Simulation.Core;
using UnityEngine;

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

            // Стартуем минимальный каркас симуляции: пока заглушки.
            _simulationClock = new SimulationClock(Time.fixedDeltaTime);
            _simulation = new SimulationRootController(_gameState, _simulationClock);

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
        }

        private IEnumerator LoadMainMenuDelayed()
        {
            yield return new WaitForSeconds(1f);
            SceneController.Load(SceneId.GalaxyMap);
        }
    }
}
