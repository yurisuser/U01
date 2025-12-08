namespace _Project.Scripts.Simulation.Core
{
    /// <summary>Типы событий симуляции.</summary>
    public enum SimulationEventType
    {
        None = 0,
        ShipSpawned,
        ShipDestroyed,
        ShipDeparted,
        ShipArrived,
        ShotFired,
        Custom = 999
    }
}
