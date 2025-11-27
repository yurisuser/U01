using _Project.Scripts.Core.GameState;

namespace _Project.Scripts.Simulation.Core
{
    /// <summary>Данные одного шага симуляции.</summary>
    public readonly struct SimulationStepContext
    {
        public SimulationStepContext(GameStateService gameState, int day, float deltaTime, ERunMode runMode)
        {
            GameState = gameState;
            Day = day;
            DeltaTime = deltaTime;
            RunMode = runMode;
        }

        /// <summary>Глобальное состояние игры.</summary>
        public GameStateService GameState { get; }

        /// <summary>Текущий номер хода/дня.</summary>
        public int Day { get; }

        /// <summary>Длительность шага для локальной симуляции.</summary>
        public float DeltaTime { get; }

        /// <summary>Режим выполнения на этот шаг.</summary>
        public ERunMode RunMode { get; }

        /// <summary>Находимся ли в паузе.</summary>
        public bool IsPaused => RunMode == ERunMode.Paused;
    }
}
