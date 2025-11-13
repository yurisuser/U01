using System;
using UnityEngine;
using _Project.Scripts.Core;

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
        public UID CurrentTarget => _orderParam.Target;
        internal bool HasCurrentTarget => IsValidUid(_orderParam.Target);

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

        internal void SetCurrentTarget(in UID target)
        {
            _orderParam.Target = target;
        }

        internal void ClearCurrentTarget()
        {
            _orderParam.Target = default;
        }

        internal bool EnsureAttackTargetAction()
        {
            if (_order != EPilotOrder.AttackTarget && _order != EPilotOrder.AttackAllEnemies)
                return false;

            if (!HasCurrentTarget)
                return false;

            var desired = PilotAction.CreateAttackTarget(
                _orderParam.Target,
                _orderParam.DesiredRange,
                _orderParam.AllowFriendlyFire);

            return EnsureOrReplaceAction(in desired);
        }

        internal bool EnsureAcquireAction()
        {
            if (_order != EPilotOrder.AttackAllEnemies)
                return false;

            var desired = PilotAction.CreateAcquireTarget(
                _orderParam.Distance,
                _orderParam.AllowFriendlyFire);

            return EnsureOrReplaceAction(in desired);
        }

        internal bool EnsureAttackAllFlow()
        {
            if (_order != EPilotOrder.AttackAllEnemies)
                return false;

            return HasCurrentTarget ? EnsureAttackTargetAction() : EnsureAcquireAction();
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

        private bool EnsureOrReplaceAction(in PilotAction desired)
        {
            EnsureInitialized();

            if (_actions.Count == 0)
            {
                _actions.Push(in desired);
                return true;
            }

            if (_actions.TryPeek(out var current))
            {
                if (AreActionsEquivalent(in current, in desired))
                    return true;

                _actions.ReplaceTop(in desired);
                return true;
            }

            _actions.Push(in desired);
            return true;
        }

        private static bool AreActionsEquivalent(in PilotAction current, in PilotAction desired)
        {
            if (current.Action != desired.Action)
                return false;

            switch (current.Action)
            {
                case EAction.MoveToCoordinates:
                    return (current.Parameters.Move.Destination - desired.Parameters.Move.Destination).sqrMagnitude <= 0.0001f &&
                           Math.Abs(current.Parameters.Move.DesiredSpeed - desired.Parameters.Move.DesiredSpeed) <= 0.0001f &&
                           Math.Abs(current.Parameters.Move.ArriveDistance - desired.Parameters.Move.ArriveDistance) <= 0.0001f;
                case EAction.AttackTarget:
                    return current.Parameters.Attack.Target.Id == desired.Parameters.Attack.Target.Id &&
                           current.Parameters.Attack.Target.Type == desired.Parameters.Attack.Target.Type &&
                           Math.Abs(current.Parameters.Attack.DesiredRange - desired.Parameters.Attack.DesiredRange) <= 0.0001f &&
                           current.Parameters.Attack.AllowFriendlyFire == desired.Parameters.Attack.AllowFriendlyFire;
                case EAction.AcquireTarget:
                    return Math.Abs(current.Parameters.Acquire.SearchRadius - desired.Parameters.Acquire.SearchRadius) <= 0.0001f &&
                           current.Parameters.Acquire.AllowFriendlyFire == desired.Parameters.Acquire.AllowFriendlyFire;
                default:
                    return false;
            }
        }

        private static bool IsValidUid(in UID uid)
        {
            return uid.Id != 0;
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
