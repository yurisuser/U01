using _Project.Scripts.Core.GameState;
using _Project.Scripts.Core.Runtime;

namespace _Project.Scripts.Simulation
{
    public sealed class Executor
    {
        private readonly RuntimeContext _context;

        public Executor(RuntimeContext context)
        {
            _context = context;
        }

        public void Execute(ref GameStateService.Snapshot snapshot, float dt)
        {
            if (_context != null)
            {
                _context.Tasks.Tick(dt);
                _context.Fleets.Tick(dt);
            }

            DoLogicStep(ref snapshot, dt);
        }

        private static void DoLogicStep(ref GameStateService.Snapshot snapshot, float dt)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"Logic tick: {snapshot.TickIndex}, dt={dt:0.###}");
#endif
        }
    }
}
