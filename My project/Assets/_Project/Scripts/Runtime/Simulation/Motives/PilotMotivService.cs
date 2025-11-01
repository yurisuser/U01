using _Project.Scripts.Ships;
using UnityEngine;

namespace _Project.Scripts.Simulation.Motives
{
    /// <summary>
    /// Управляет созданием и обновлением мотивов пилота.
    /// </summary>
    public sealed class PilotMotivService
    {
        private readonly float _defaultPatrolRadius;
        private readonly float _arriveDistance;

        public PilotMotivService(float defaultPatrolRadius, float arriveDistance)
        {
            _defaultPatrolRadius = Mathf.Max(arriveDistance, defaultPatrolRadius);
            _arriveDistance = Mathf.Max(0.01f, arriveDistance);
        }

        public PilotMotiv CreateDefaultPatrol(Vector3 center, float desiredSpeed)
        {
            return CreatePatrol(center, _defaultPatrolRadius, desiredSpeed);
        }

        public PilotMotiv CreatePatrol(Vector3 center, float radius, float desiredSpeed)
        {
            var patrolRadius = Mathf.Max(radius, _arriveDistance * 2f);
            var motiv = new PilotMotiv
            {
                Order = PilotOrderType.Patrol,
                DesiredSpeed = desiredSpeed,
                WaitTimer = 0f,
                Patrol = new PilotMotiv.PatrolState
                {
                    Center = center,
                    Radius = patrolRadius,
                    CurrentTarget = Vector3.zero,
                    HasTarget = false
                }
            };

            AssignNextPatrolTarget(ref motiv, center);
            return motiv;
        }

        public void UpdatePatrol(ref Ship ship, ref PilotMotiv motiv, float dt)
        {
            if (motiv.Order != PilotOrderType.Patrol)
                return;

            EnsureTarget(ref motiv, ship.Position);

            var target = motiv.Patrol.CurrentTarget;
            var toTarget = target - ship.Position;
            var distance = toTarget.magnitude;

            if (distance <= _arriveDistance)
            {
                AssignNextPatrolTarget(ref motiv, ship.Position);
                target = motiv.Patrol.CurrentTarget;
                toTarget = target - ship.Position;
                distance = toTarget.magnitude;

                if (distance <= Mathf.Epsilon)
                    return;
            }

            float speed = Mathf.Max(0.1f, motiv.DesiredSpeed);
            var direction = toTarget.normalized;
            var move = direction * speed * dt;
            if (move.magnitude > distance)
                move = direction * distance;

            ship.Position += move;
            float angleDeg = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            ship.Rotation = Quaternion.Euler(0f, 0f, angleDeg);
        }

        private void EnsureTarget(ref PilotMotiv motiv, Vector3 origin)
        {
            if (!motiv.Patrol.HasTarget)
                AssignNextPatrolTarget(ref motiv, origin);
        }

        private void AssignNextPatrolTarget(ref PilotMotiv motiv, Vector3 origin)
        {
            var patrol = motiv.Patrol;
            var center = patrol.Center;
            if (center == Vector3.zero && patrol.HasTarget == false)
                center = origin;

            var next = PickPointWithinRadius(center, patrol.Radius);
            for (int i = 0; i < 5 && (next - origin).sqrMagnitude < _arriveDistance * _arriveDistance; i++)
                next = PickPointWithinRadius(center, patrol.Radius);

            patrol.CurrentTarget = next;
            patrol.HasTarget = true;
            motiv.Patrol = patrol;
        }

        private static Vector3 PickPointWithinRadius(Vector3 center, float radius)
        {
            var offset = Random.insideUnitCircle * radius;
            return new Vector3(center.x + offset.x, center.y + offset.y, center.z);
        }
    }
}
