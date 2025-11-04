using System;
using UnityEngine;

namespace _Project.Scripts.Simulation.PilotMotivation
{
    /// <summary>
    /// Value-type state that describes the current pilot order and expands it to executable actions.
    /// </summary>
    public struct PilotMotive
    {
        private EPilotOrder _order;
        private ActionParam _orderParam;
        private PilotActionStack _actions;
        private PatrolState _patrol;
        private bool _initialized;

        public EPilotOrder Order => _order;
        public ActionParam OrderParameters => _orderParam;
        public int ActionCount => _actions.Count;
        public bool IsInitialized => _initialized;

        public void Reset(int actionCapacity = 16)
        {
            _actions.Initialize(actionCapacity);
            _order = EPilotOrder.Idle;
            _orderParam = default;
            _patrol = default;
            _initialized = true;
        }

        public void SetOrder(EPilotOrder order, in ActionParam param, int actionCapacity = 16)
        {
            EnsureInitialized(actionCapacity);
            _order = order;
            _orderParam = param;
            _actions.Clear();
            _patrol = default;
        }

        public bool TryPeekAction(out PilotAction action)
        {
            EnsureInitialized();
            return _actions.TryPeek(out action);
        }

        public bool TryPopAction(out PilotAction action)
        {
            EnsureInitialized();
            return _actions.TryPop(out action);
        }

        public void CompleteCurrentAction()
        {
            if (!TryPopAction(out _))
                return;

            if (_order == EPilotOrder.Patrol)
            {
                var patrol = _patrol;
                patrol.HasTarget = false;
                _patrol = patrol;
            }
        }

        internal void ConfigurePatrol(Vector3 center, float radius, float desiredSpeed, float arriveDistance, uint randomState)
        {
            EnsureInitialized();

            var patrolRadius = Math.Max(radius, arriveDistance * 2f);
            _patrol = new PatrolState
            {
                Center = center,
                Radius = patrolRadius,
                DesiredSpeed = Math.Max(0.1f, desiredSpeed),
                ArriveDistance = Math.Max(0.01f, arriveDistance),
                RandomState = randomState,
                CurrentTarget = Vector3.zero,
                HasTarget = false
            };
        }

        internal bool EnsurePatrolAction(Vector3 origin)
        {
            if (_order != EPilotOrder.Patrol)
                return false;

            EnsurePatrolTarget(origin);

            var desired = PilotAction.CreateMoveTo(
                _patrol.CurrentTarget,
                _patrol.DesiredSpeed,
                _patrol.ArriveDistance);

            if (_actions.Count == 0)
            {
                _actions.Push(in desired);
                return true;
            }

            if (_actions.TryPeek(out var current))
            {
                if (current.Action != desired.Action ||
                    (current.Parameters.Move.Destination - desired.Parameters.Move.Destination).sqrMagnitude > 0.0001f ||
                    Math.Abs(current.Parameters.Move.DesiredSpeed - desired.Parameters.Move.DesiredSpeed) > 0.0001f ||
                    Math.Abs(current.Parameters.Move.ArriveDistance - desired.Parameters.Move.ArriveDistance) > 0.0001f)
                {
                    _actions.ReplaceTop(in desired);
                }

                return true;
            }

            _actions.Push(in desired);
            return true;
        }

        private void EnsurePatrolTarget(Vector3 origin)
        {
            var patrol = _patrol;
            if (!patrol.HasTarget)
            {
                AssignNextPatrolTarget(ref patrol, origin);
                _patrol = patrol;
                return;
            }

            var toTarget = patrol.CurrentTarget - origin;
            if (toTarget.sqrMagnitude <= patrol.ArriveDistance * patrol.ArriveDistance)
            {
                AssignNextPatrolTarget(ref patrol, origin);
                _patrol = patrol;
            }
        }

        private static void AssignNextPatrolTarget(ref PatrolState patrol, Vector3 origin)
        {
            var center = patrol.Center;
            if (!patrol.HasTarget && center == Vector3.zero)
                center = origin;

            var randomState = patrol.RandomState;
            var next = PickPointWithinRadius(ref randomState, center, patrol.Radius);
            var minDistanceSqr = patrol.ArriveDistance * patrol.ArriveDistance;
            for (int i = 0; i < 5 && (next - origin).sqrMagnitude < minDistanceSqr; i++)
                next = PickPointWithinRadius(ref randomState, center, patrol.Radius);

            patrol.Center = center;
            patrol.RandomState = randomState;
            patrol.CurrentTarget = next;
            patrol.HasTarget = true;
        }

        private static Vector3 PickPointWithinRadius(ref uint state, Vector3 center, float radius)
        {
            var offset = SamplePointOnDisk(ref state, radius);
            return new Vector3(center.x + offset.x, center.y + offset.y, center.z);
        }

        private static Vector2 SamplePointOnDisk(ref uint state, float radius)
        {
            float angle = NextFloat(ref state) * Mathf.PI * 2f;
            float distance = Mathf.Sqrt(NextFloat(ref state)) * radius;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        }

        private static float NextFloat(ref uint state)
        {
            state = NextState(state);
            return (state & 0x00FFFFFFu) / 16777216f;
        }

        private static uint NextState(uint state)
        {
            if (state == 0u)
                state = 0x9E3779B9u;

            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }

        private void EnsureInitialized(int actionCapacity = 16)
        {
            if (!_initialized)
                Reset(actionCapacity);
        }

        private struct PatrolState
        {
            public Vector3 Center;
            public Vector3 CurrentTarget;
            public float Radius;
            public float DesiredSpeed;
            public float ArriveDistance;
            public uint RandomState;
            public bool HasTarget;
        }
    }
}
