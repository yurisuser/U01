using _Project.Scripts.Core.GameState;
using _Project.Scripts.Core.Runtime;

namespace _Project.Scripts.Simulation
{
    /// <summary>
    /// Центральный исполнитель игрового шага.
    /// </summary>
    public sealed class Executor
    {
        private readonly RuntimeContext _context;
        private readonly GameStateService _state;

        public Executor(RuntimeContext context, GameStateService state)
        {
            _context = context;
            _state = state;
        }

        public void Execute(ref GameStateService.Snapshot snapshot, float dt)
        {
            if (_context != null)
            {
                _context.Tasks.Tick(dt);
                _context.Ships.Tick(dt); // здесь позже появится логика движения
            }

            DoLogicStep(ref snapshot, dt);

            // после всех изменений синхронизируем снапшот для рендера
            _state?.RefreshDynamicSnapshot();
        }

        private static void DoLogicStep(ref GameStateService.Snapshot snapshot, float dt)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"Logic tick: {snapshot.TickIndex}, dt={dt:0.###}");
#endif
        }
    }
}
