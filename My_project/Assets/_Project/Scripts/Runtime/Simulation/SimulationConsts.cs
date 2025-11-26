namespace _Project.Scripts.Simulation
{
    /// <summary>Базовые константы симуляции, оставленные для статических слоёв.</summary>
    public static class SimulationConsts
    {
        /// <summary>Радиус внутренней мёртвой зоны (в орбитальных единицах).</summary>
        public const float InnerDeadZoneOrbits = 1f;

        /// <summary>Сколько кораблей создавать на систему при старте.</summary>
        public const int ShipsPerSystem = 4;

        /// <summary>Базовый радиус спавна кораблей.</summary>
        public const float SpawnRadius = 5f;
    }
}
