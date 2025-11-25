namespace _Project.Scripts.Simulation
{
    /// <summary>
    /// Глобальные постоянные симуляции, сгруппированные по назначению.
    /// </summary>
    public static class SimulationConsts
    {
        // ----- Спавн флота -----
        public const int   ShipsPerSystem     = 5;     // сколько кораблей спауним на систему при старте
        public const float SpawnRadius        = 6f;    // базовый радиус спавна вокруг центра системы (условные ед.)

        // ----- Патруль/боевое поведение -----
        public const float ArriveDistance     = 0.2f;  // дистанция, с которой цель считается достигнутой
        public const float DefaultPatrolSpeed = 5f;    // желаемая скорость патруля по умолчанию
        public const float DefaultPatrolRadius = 200f; // радиус патрулирования по умолчанию

        // ----- Мёртвые зоны систем -----
        public const float InnerDeadZoneOrbits = 2f;   // внутренний радиус мёртвой зоны (в орбитах)
        public const float OuterDeadZoneOrbits = 25f;  // внешний радиус мёртвой зоны (в орбитах)
    }
}
