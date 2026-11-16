namespace _Project.Scripts.Const
{
    /// <summary>Минимальные константы симуляции, оставленные для статики и спавна.</summary>
    public static class SimulationConsts
    {
        // --- Спавн ---
        public const int ShipsPerSystem = 5;
        public const float SpawnRadius = 200f;

        // --- Ship tasks ---
        public const float WarpSpeedMultiplier = 1f; // коэффициент скорости для прямолинейного варп-движения
        public const float MetricSpaceSpeedMultiplier = 0.001f; // коэффициент скорости для манёвров в метрическом пространстве

        // --- Внутрисистемный варп ---
        public const int WarpChargeTurns = 3; // полные игровые ходы заряда варп-двигателя
        public const int MetricBrakeTurns = 1; // полный игровой ход неуправляемого торможения в метрике
        public const float WarpCourseToleranceDegrees = 5f; // допустимое отклонение курса перед началом заряда
        public const float WarpExitRadiusMetric = 0.2f; // временный фиксированный радиус автоматического выхода в метрике

        // --- Прогноз перемещения ---
        public const int TravelPredictionMaxSteps = 100000; // ограничение работы одного варианта; исчерпание не означает недостижимость
        public const float MovementSpeedEpsilon = 0.0001f; // численный допуск достижения максимальной скорости
        public const float MovementPositionEpsilon = 0.0001f; // численный допуск достижения точки выхода из варпа

        public const float DestinationPointTolerance = 10f; //допустимость неточности достижения координат
        public const float AccelerationOfAgility = 8; // умножить на агилити для получения разгона/торможения
        public const float AgilityTurnConeFactor = 10f; // множитель ширины конуса разворота

        // --- Тайминги ---
        public const float GlobalStepSeconds = 2f; // длительность хода глобальной симуляции (1 ход = 1 день)
    }
}
