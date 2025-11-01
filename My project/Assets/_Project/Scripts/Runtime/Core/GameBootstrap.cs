using System.Collections;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Core.Input;
using _Project.Scripts.Core.Runtime;
using _Project.Scripts.Core.Scene;
using _Project.Scripts.Galaxy.Generation;
using _Project.Scripts.Simulation;
using UnityEngine;

namespace _Project.Scripts.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        private static GameStateService _gameState;

        public static GameStateService GameState
        {
            get
            {
                if (_gameState == null)
                    _gameState = new GameStateService(2.0f);

                return _gameState;
            }
        }

        public static GameBootstrap Instance { get; private set; }

        public SceneController Scenes { get; } = new SceneController();
        public InputController Input { get; } = new InputController();

        [SerializeField] private float stepDurationSeconds = 2f;

        private StepManager _stepManager;
        private Executor _executor;

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
                _gameState = new GameStateService(stepDurationSeconds);
            else
                _gameState.SetLogicStepSeconds(stepDurationSeconds);

            var galaxy = GalaxyCreator.Create();
            var context = RuntimeWorldService.RequireContext();
            context.Initialize(galaxy);

            _gameState.SetGalaxy(galaxy);
            _gameState.AttachRuntimeContext(context);

            _executor = new Executor(context);
            _stepManager = new StepManager(_gameState, _executor);

            StartCoroutine(LoadMainMenuDelayed());
        }

        private void Update()
        {
            Input?.Update();
            _stepManager?.Update(Time.deltaTime);
        }

        private IEnumerator LoadMainMenuDelayed()
        {
            yield return new WaitForSeconds(1f);
            SceneController.Load(SceneId.GalaxyMap);
        }
    }
}
