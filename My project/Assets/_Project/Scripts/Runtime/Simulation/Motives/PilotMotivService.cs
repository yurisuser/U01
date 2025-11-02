using System;
using System.Threading;
using _Project.Scripts.Ships;
using UnityEngine;

namespace _Project.Scripts.Simulation.Motives
{
    // Управляет созданием и обновлением мотивов пилота.
    public sealed class PilotMotivService
    {
        private static int _seedCounter = Environment.TickCount;

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
            var motiv = PilotMotiv.Idle();
            motiv.Mission = PilotMotiv.MissionState.CreatePatrol(center, patrolRadius);

            var frame = PilotMotiv.TaskFrame.CreatePatrolMove(desiredSpeed, CreateSeed());
            var execution = motiv.Execution;
            var stack = execution.TaskStack;
            stack.Push(in frame);
            execution.TaskStack = stack;
            motiv.Execution = execution;

            AssignNextPatrolTarget(ref motiv, center);
            return motiv;
        }

        public void UpdatePatrol(ref Ship ship, ref PilotMotiv motiv, float dt)
        {
            if (motiv.Mission.Kind != EPilotTasks.Patrol)
                return;

            var execution = motiv.Execution;
            var stack = execution.TaskStack;
            if (stack.Count == 0)
                return;

            var frame = stack.Peek();
            if (frame.Kind != EPilotSubTasks.PatrolMove)
                return;

            EnsureTarget(ref motiv, ship.Position);

            execution = motiv.Execution;
            stack = execution.TaskStack;
            frame = stack.Peek();
            var patrolTask = frame.Payload.Patrol;

            var target = patrolTask.CurrentTarget;
            var toTarget = target - ship.Position;
            var distance = toTarget.magnitude;

            if (distance <= _arriveDistance)
            {
                AssignNextPatrolTarget(ref motiv, ship.Position);

                execution = motiv.Execution;
                stack = execution.TaskStack;
                frame = stack.Peek();
                patrolTask = frame.Payload.Patrol;

                target = patrolTask.CurrentTarget;
                toTarget = target - ship.Position;
                distance = toTarget.magnitude;

                if (distance <= Mathf.Epsilon)
                    return;
            }

            float speed = Mathf.Max(0.1f, patrolTask.DesiredSpeed);
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
            var execution = motiv.Execution;
            var stack = execution.TaskStack;
            if (stack.Count == 0)
                return;

            var frame = stack.Peek();
            if (frame.Kind != EPilotSubTasks.PatrolMove)
                return;

            if (!frame.Payload.Patrol.HasTarget)
                AssignNextPatrolTarget(ref motiv, origin);
        }

        private void AssignNextPatrolTarget(ref PilotMotiv motiv, Vector3 origin)
        {
            var mission = motiv.Mission;
            if (mission.Kind != EPilotTasks.Patrol)
                return;

            var execution = motiv.Execution;
            var stack = execution.TaskStack;
            if (stack.Count == 0)
                return;

            var frame = stack.Peek();
            if (frame.Kind != EPilotSubTasks.PatrolMove)
                return;

            var patrolMission = mission.Parameters.Patrol;
            var patrolTask = frame.Payload.Patrol;

            var center = patrolMission.Center;
            if (center == Vector3.zero && !patrolTask.HasTarget)
            {
                center = origin;
                patrolMission.Center = center;
                mission.Parameters.Patrol = patrolMission;
                motiv.Mission = mission;
            }

            var state = patrolTask.RandomState;
            var next = PickPointWithinRadius(ref state, center, patrolMission.Radius);
            for (int i = 0; i < 5 && (next - origin).sqrMagnitude < _arriveDistance * _arriveDistance; i++)
                next = PickPointWithinRadius(ref state, center, patrolMission.Radius);

            patrolTask.RandomState = state;
            patrolTask.CurrentTarget = next;
            patrolTask.HasTarget = true;

            frame.Payload.Patrol = patrolTask;
            stack.ReplaceTop(in frame);
            execution.TaskStack = stack;
            motiv.Execution = execution;
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

        private static uint CreateSeed()
        {
            var value = unchecked((uint)Interlocked.Increment(ref _seedCounter));
            return value == 0u ? 0xA511E9B3u : value;
        }
    }
}
