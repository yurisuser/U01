namespace _Project.Scripts.Simulation
{
    /// <summary>
    /// Какие задачи может выполнять пилот прямо сейчас.
    /// </summary>
    public enum PilotOrderType
    {
        Idle = 0,   // просто стоим на месте
        Patrol = 1  // облет по заранее заданным точкам
    }
}
