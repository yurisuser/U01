namespace _Project.Scripts.Const
{
    /// <summary>Константы для слоя Continuum.</summary>
    public static class ContinuumConsts
    {
        public const float EntryZoneRadius = 20f;            // Радиус круга переходной зоны
        public const float EntryZoneOffset = 30f;            // Отступ от последней орбиты/звезды до центра зоны
        public const int JumpDurationTurns = 3;              // Длительность прыжка в ходах
        public const int ArrivalMinOrbitIndex = 1;           // Минимальный номер орбиты для выхода
        public const int ArrivalMaxOrbitIndex = 3;           // Максимальный номер орбиты для выхода
        public const int ApproachTurns = 2;                   // Подлёты в системе: до зоны и от выхода до станции (упрощённо)
        public const int JumpExitOrbitIndex = 3;              // Орбита выхода по умолчанию
    }
}
