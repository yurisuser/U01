using _Project.Scripts.Core.GameState;
using _Project.Scripts.Core.Runtime;
using _Project.Scripts.Simulation;

namespace _Project.Scripts.Simulation.Execution
{
    /// <summary>Исполняет игровой шаг: обновляет задачи, двигает корабли и синхронизирует UI.</summary>
    public sealed class Executor
    {
        private readonly RuntimeContext _context; // Контекст мира.
        private readonly GameStateService _state; // Сервис состояния.
        private readonly Spawn.ShipSpawnService _shipSpawner; // Сервис первичного спавна.

        // Готовим все зависимости исполнения шага.
        public Executor(RuntimeContext context, GameStateService state) //Конструктор
        {
            _context = context;
            _state = state;
            _shipSpawner = new Spawn.ShipSpawnService(_context);
        }

        // Выполняем один логический ход симуляции.
        public void Execute(ref Snapshot snapshot, float dt)
        {
            _shipSpawner.EnsureInitialShips();

            if (_context != null)
            {
                _context.Tasks.Tick(dt);
                _context.Ships.Tick(dt);
                // Временная заглушка: старый ИИ выключен, корабли не обновляем.
            }

            DoLogicStep(ref snapshot, dt);
            _state?.MarkDynamicDirty();
        }

        // Отдельный шаг логики (для отладки в редакторе).
        private static void DoLogicStep(ref Snapshot snapshot, float dt)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"Logic tick: {snapshot.TickIndex}, dt={dt:0.###}");
#endif
        }
    }
}
