using _Project.Scripts.Core.GameState;

namespace _Project.Scripts.Simulation
{
    public sealed class Executor
    {
        public void Execute(ref GameStateService.Snapshot snapshot, float dt) // dt - продолжительность логического шага
        {
            DoLogicStep(ref snapshot, dt); // здесь будет основная симуляция
        }

        private void DoLogicStep(ref GameStateService.Snapshot snapshot, float dt)
        {
            UnityEngine.Debug.Log($"Logic tick: {snapshot.TickIndex}, dt={dt:0.###}"); // временная заглушка
        }
    }
}
