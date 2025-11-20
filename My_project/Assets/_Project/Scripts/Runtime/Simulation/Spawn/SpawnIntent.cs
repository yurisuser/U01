namespace _Project.Scripts.Simulation.Spawn
{
    /// <summary>Минимальный DTO намерения спауна.</summary>
    public readonly struct SpawnIntent
    {
        public readonly int SystemId;     // где спаунить
        public readonly string Role;      // "Patrol" и т.п. — строкой на первое время

        /// <summary>Создаёт намерение спауна по системе и роли.</summary>
        public SpawnIntent(int systemId, string role)
        {
            SystemId = systemId;
            Role = role;
        }
    }
}
