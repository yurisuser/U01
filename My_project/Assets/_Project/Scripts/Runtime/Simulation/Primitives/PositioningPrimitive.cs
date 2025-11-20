using _Project.Scripts.Core;
using UnityEngine;

namespace _Project.Scripts.Simulation.Primitives
{
    /// <summary>Утилиты позиционирования относительно цели.</summary>
    internal static class PositioningPrimitive
    {
        private const float OrbitLateralFactor = 0.35f; // Насколько далеко смещаемся по касательной.

        // Расстояние между атакующим и целью.
        public static float DistanceToTarget(Vector3 attackerPos, TargetSnapshot target)
        {
            return Vector3.Distance(attackerPos, target.Position);
        }

        // Точка погони прямо позади цели.
        public static Vector3 ComputeChasePoint(Vector3 attackerPos, TargetSnapshot target, float desiredDistance)
        {
            var toTarget = target.Position - attackerPos;
            if (toTarget.sqrMagnitude <= Mathf.Epsilon)
                return target.Position;

            var dir = toTarget.normalized;
            return target.Position - dir * desiredDistance;
        }

        // Точка орбиты с боковым смещением для разных кораблей.
        public static Vector3 ComputeOrbitPoint(in UID attackerUid, Vector3 attackerPos, TargetSnapshot target, float radius)
        {
            var toTarget = target.Position - attackerPos;
            Vector3 dir = toTarget.sqrMagnitude > Mathf.Epsilon ? toTarget.normalized : Vector3.right;
            Vector3 tangent = new Vector3(-dir.y, dir.x, 0f);

            float hash = HashPair(attackerUid, target.Uid) * 2f - 1f; // [-1;1]
            var chasePoint = target.Position - dir * radius;
            return chasePoint + tangent * radius * OrbitLateralFactor * hash;
        }

        // Хеш-комбинация UID для детерминированного, но уникального отступа.
        private static float HashPair(in UID attacker, in UID target)
        {
            unchecked
            {
                uint value = (uint)attacker.Id * 1103515245u;
                value ^= (uint)attacker.Type << 16;
                value ^= (uint)target.Id * 2654435761u;
                value ^= (uint)target.Type << 8;
                value ^= value << 13;
                value ^= value >> 17;
                value ^= value << 5;
                return (value & 0x00FFFFFFu) / 16777216f;
            }
        }
    }
}
