namespace _Project.Scripts.Simulation.Spawn
{
    // Минимальный DTO намерения спауна
    public readonly struct SpawnIntent
    {
        public readonly int SystemId;     // где спаунить
        public readonly string Role;      // "Patrol" и т.п. — строкой на первое время

        public SpawnIntent(int systemId, string role)
        {
            SystemId = systemId;
            Role = role;
        }
    }
}