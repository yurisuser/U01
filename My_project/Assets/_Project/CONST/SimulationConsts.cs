namespace _Project.Scripts.Const
{
    /// <summary>Минимальные константы симуляции, оставленные для статики и спавна.</summary>
    public static class SimulationConsts
    {
        // --- Геометрия и ограничения ---
        public const float InnerDeadZoneOrbits = 1f;

        // --- Спавн ---
        public const int ShipsPerSystem = 5;
        public const float SpawnRadius = 200f;

        // --- Ship tasks ---
        public const float DestinationPointTolerance = 10f; //допустимость неточности достижения координат
        public const float AccelerationOfAgility = 8; // умножить на агилити для получения разгона/торможения
        public const float AgilityTurnConeFactor = 10f; // множитель ширины конуса разворота

        // --- Тайминги ---
        public const float GlobalStepSeconds = 2f; // длительность хода глобальной симуляции (1 ход = 1 день)
    }
}
