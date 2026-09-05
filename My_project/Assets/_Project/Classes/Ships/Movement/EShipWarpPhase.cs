namespace _Project.Scripts.Ships
{
    /// <summary>Текущая фаза внутрисистемного варп-движения корабля.</summary>
    public enum EShipWarpPhase
    {
        Metric = 0,
        Charging = 1,
        Warp = 2,
        MetricBrake = 3,
    }
}
