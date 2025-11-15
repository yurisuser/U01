using UnityEngine;

namespace _Project.Scripts.Simulation.Primitives
{
    internal static class DistanceToTargetPrimitive
    {
        public static float Compute(Vector3 attackerPos, TargetSnapshot target)
        {
            return Vector3.Distance(attackerPos, target.Position);
        }
    }
}
