using _Project.Scripts.Ships;
using UnityEngine;

namespace _Project.Scripts.Simulation.Primitives
{
    internal static class MovementPrimitive
    {
        public static bool MoveToPosition(ref Ship ship, in Vector3 target, float desiredSpeed, float arriveDistance, float dt)
        {
            arriveDistance = Mathf.Max(arriveDistance, 0.01f);
            var toTarget = target - ship.Position;
            var distance = toTarget.magnitude;

            desiredSpeed = Mathf.Max(desiredSpeed, 0.1f);
            if (ship.Stats.MaxSpeed > 0f)
                desiredSpeed = Mathf.Min(desiredSpeed, ship.Stats.MaxSpeed);

            if (distance <= arriveDistance)
            {
                ship.Position = target;
                ship.Velocity = Vector3.zero;
                return true;
            }

            var forward = ship.Rotation * Vector3.right;
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.right;
            forward.Normalize();

            float turnRadius = ship.Stats.Agility > 0f ? 1f / ship.Stats.Agility : float.PositiveInfinity;

            const float MaxSubstep = 0.1f;
            int steps = Mathf.Clamp(Mathf.CeilToInt(dt / MaxSubstep), 1, 60);
            float subDt = dt / steps;
            bool reachedTarget = false;

            for (int i = 0; i < steps; i++)
            {
                toTarget = target - ship.Position;
                distance = toTarget.magnitude;

                if (distance <= arriveDistance)
                {
                    ship.Position = target;
                    reachedTarget = true;
                    break;
                }

                var desiredDir = distance > Mathf.Epsilon ? toTarget / distance : Vector3.zero;

                if (desiredDir.sqrMagnitude > Mathf.Epsilon && !float.IsInfinity(turnRadius))
                {
                    float maxTurnRate = desiredSpeed / Mathf.Max(turnRadius, 0.0001f);
                    float maxTurn = maxTurnRate * subDt;
                    forward = Vector3.RotateTowards(forward, desiredDir, maxTurn, 0f).normalized;
                }

                float subDistance = desiredSpeed * subDt;

                if (!float.IsInfinity(turnRadius) && desiredDir.sqrMagnitude > Mathf.Epsilon)
                {
                    float distanceAlongForward = Vector3.Dot(toTarget, forward);
                    if (distanceAlongForward <= 0f)
                        continue;

                    if (subDistance > distanceAlongForward)
                        subDistance = distanceAlongForward;
                }

                ship.Position += forward * subDistance;
            }

            if (forward.sqrMagnitude > Mathf.Epsilon)
            {
                float angleDeg = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
                ship.Rotation = Quaternion.Euler(0f, 0f, angleDeg);
            }

            var remaining = target - ship.Position;
            if (reachedTarget || remaining.sqrMagnitude <= arriveDistance * arriveDistance)
            {
                ship.Position = target;
                ship.Velocity = Vector3.zero;
                return true;
            }

            ship.Velocity = forward * desiredSpeed;
            return false;
        }

        public static void Stop(ref Ship ship)
        {
            ship.Velocity = Vector3.zero;
        }
    }
}
