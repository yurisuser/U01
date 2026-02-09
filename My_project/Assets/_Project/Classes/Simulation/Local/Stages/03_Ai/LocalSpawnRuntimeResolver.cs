using _Project.Scripts.Simulation;
using _Project.Scripts.Stations;

namespace _Project.Scripts.Simulation.Local.Stages.Ai
{
    /// <summary>Разрешение runtime активной системы для spawn-пайплайна.</summary>
    internal static class LocalSpawnRuntimeResolver
    {
        public static bool TryResolve(
            in LocalSimulationContext context,
            out LocalSysRuntimeContext runtime,
            out Station[] stations,
            out int systemIndex)
        {
            runtime = null;
            stations = null;
            systemIndex = -1;

            var gameState = context.GameState;
            if (gameState == null)
                return false; // Нет геймстейта.

            var galaxy = gameState.Galaxy;
            if (galaxy == null || galaxy.Length == 0)
                return false; // Пустая галактика.

            int index = gameState.SelectedSystemIndex;
            if (index < 0 || index >= galaxy.Length)
                return false; // Индекс активной системы вне диапазона.

            var system = galaxy[index];
            if (system.State == null)
            {
                system.State = new LocalSysRuntimeContext(); // Ленивая инициализация runtime.
                galaxy[index] = system;
            }

            runtime = system.State;     // Динамическое состояние (ships/snapshots).
            stations = system.Stations; // Статические станции системы.
            systemIndex = index;        // Индекс системы для top-order параметров.
            return true;
        }
    }
}
