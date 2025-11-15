using _Project.Scripts.Core;
using UnityEngine;

namespace _Project.Scripts.Simulation.Primitives
{
    internal static class ComputeOrbitPointPrimitive
    {
        public static Vector3 Compute(in UID attackerUid, Vector3 attackerPos, TargetSnapshot target, float radius)
        {
            radius = Mathf.Max(0.01f, radius);
            var center = target.Position;
            var toAttacker = attackerPos - center;
            float dist = toAttacker.magnitude;
            float side = HashPair(attackerUid, target.Uid) < 0.5f ? -1f : 1f;

            if (dist > radius + 0.1f)
            {
                float baseAngle = Mathf.Atan2(toAttacker.y, toAttacker.x);
                float offset = Mathf.Acos(Mathf.Clamp(radius / dist, -1f, 1f));
                float tangentAngle = baseAngle + side * offset;
                return center + new Vector3(Mathf.Cos(tangentAngle), Mathf.Sin(tangentAngle), 0f) * radius;
            }

            Vector3 radial = dist > 0.001f ? toAttacker.normalized : Vector3.right;
            float currentAngle = Mathf.Atan2(radial.y, radial.x);
            float advance = side * 0.35f;
            float orbitAngle = currentAngle + advance;
            return center + new Vector3(Mathf.Cos(orbitAngle), Mathf.Sin(orbitAngle), 0f) * radius;
        }

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
