using _Project.Scripts.Core.GameState;
using _Project.Scripts.Galaxy.Data;

namespace _Project.Scripts.Simulation.Local
{
    /// <summary>Контекст локального шага: выбранная система, время и ссылки на стейт.</summary>
    public readonly struct LocalSimulationContext
    {
        public LocalSimulationContext(GameStateService gameState, StarSys? activeSystem, int day, float deltaTime)
        {
            GameState = gameState;
            ActiveSystem = activeSystem;
            Day = day;
            DeltaTime = deltaTime;
        }

        public GameStateService GameState { get; }
        public StarSys? ActiveSystem { get; }
        public int Day { get; }
        public float DeltaTime { get; }
        public bool HasActiveSystem => ActiveSystem.HasValue;
    }
}
