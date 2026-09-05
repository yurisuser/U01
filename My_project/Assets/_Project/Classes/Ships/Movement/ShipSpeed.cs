using UnityEngine;
using _Project.Scripts.Const;

namespace _Project.Scripts.Ships
{
    /// <summary>Вычисляет предельные скорости корабля для текущих режимов движения.</summary>
    public static class ShipSpeed
    {
        public static float GetMetricMaxSpeed(in Ship ship)
        {
            return Mathf.Max(0f, ship.Stats.MetricSpeed) * SimulationConsts.MetricSpaceSpeedMultiplier;
        }

        public static float GetWarpSpeed(in Ship ship)
        {
            return Mathf.Max(0f, ship.Stats.WarpSpeed) * SimulationConsts.WarpSpeedMultiplier;
        }
    }
}
