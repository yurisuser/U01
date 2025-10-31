using System.Collections;                                         // ����⨭� ����㧪� �業�
using _Project.Scripts.Core.Input;                                // ����஫��� �����
using _Project.Scripts.Core.Scene;                                // �������� �業
using _Project.Scripts.Galaxy.Generation;                         // ������� �����⨪�
using UnityEngine;                                                // Unity API
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Simulation;

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
                {
                   // Debug.LogWarning("[GameBootstrap] GameStateService requested before bootstrap initialized. Creating fallback instance.");
                    _gameState = new GameStateService(2.0f);
                }
                return _gameState;
            }
        }

        public static GameBootstrap Instance { get; private set; }         // ᨭ��⮭ ��
        public SceneController Scenes { get; } = new SceneController(); // �ࠢ����� �業���
        public InputController  Input  { get; } = new InputController(); // ���� ����� (polling)

        [SerializeField] private float stepDurationSeconds = 2f; // ���⥫쭮��� �����᪮�� 蠣�

        private StepManager _stepManager;                          // ������ ����᫥�� StepManager
        private Executor _executor;                                // ���������� �����������

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            DontDestroyOnLoad(gameObject);

            if (_gameState == null)
                _gameState = new GameStateService(stepDurationSeconds);   // �ࢨ� ���ﭨ�, 蠣 - �� ��ᯥ���
            else
                _gameState.SetLogicStepSeconds(stepDurationSeconds);     // ��������� ����� ᮢ��� �ந��

            var galaxy = GalaxyCreator.Create();                      // ᮧ��� ����� �����⨪�
            _gameState.SetGalaxy(galaxy);                             // ��࠭塞 � ���ﭨ�

            _executor    = new Executor();                            // ���������� ᮡࠣ����
            _stepManager = new StepManager(_gameState, _executor);    // ������ StepManager � сервисами

            StartCoroutine(LoadMainMenuDelayed());                   // ��� ����㧪� ��ࢮ� �業�
        }

        private void Update()
        {
            Input?.Update();                                         // ���� �����
            _stepManager?.Update(Time.deltaTime);                    // �������� ���������
        }

        private IEnumerator LoadMainMenuDelayed()                    // �������� ����প� ��� ���४⭮� ���樠����樨
        {
            yield return new WaitForSeconds(1f);
            SceneController.Load(SceneId.GalaxyMap);                 // ����᪨� �맮� ����㧪� �業�
        }
    }
}
