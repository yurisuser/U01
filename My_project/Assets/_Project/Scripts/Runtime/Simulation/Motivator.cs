using System;
using System.Threading;
using UnityEngine;
using _Project.Scripts.Simulation.PilotMotivation;

namespace _Project.Scripts.Simulation
{
    /// <summary>
    /// High-level API for assigning pilot motives and expanding orders into executable actions.
    /// </summary>
    public sealed class Motivator
    {
        private static int _seedCounter = Environment.TickCount;

        private readonly float _defaultPatrolRadius;
        private readonly float _defaultPatrolSpeed;
        private readonly float _arriveDistance;

        public Motivator(float defaultPatrolRadius, float arriveDistance, float defaultPatrolSpeed)
        {
            _defaultPatrolRadius = Mathf.Max(arriveDistance, defaultPatrolRadius);
            _defaultPatrolSpeed = Mathf.Max(0.1f, defaultPatrolSpeed);
            _arriveDistance = Mathf.Max(0.01f, arriveDistance);
        }

        public PilotMotive CreateDefaultPatrol(Vector3 origin, float desiredSpeedOverride = float.NaN)
        {
            var speed = float.IsNaN(desiredSpeedOverride)
                ? _defaultPatrolSpeed
                : Mathf.Max(0.0f, desiredSpeedOverride);

            return CreatePatrol(origin, _defaultPatrolRadius, speed, origin);
        }

        public PilotMotive CreatePatrol(Vector3 center, float radius, float desiredSpeed, Vector3 origin)
        {
            var motive = new PilotMotive();
            ConfigurePatrol(ref motive, center, radius, desiredSpeed);
            motive.EnsurePatrolAction(origin);
            return motive;
        }

        public void ConfigurePatrol(ref PilotMotive motive, Vector3 center, float radius, float desiredSpeed)
        {
            var clampedRadius = Mathf.Max(radius, _arriveDistance * 2f);
            var clampedSpeed = Mathf.Max(0.1f, desiredSpeed);
            var actionParam = new ActionParam
            {
                Coordinates = center,
                Distance = clampedRadius
            };

            motive.SetOrder(EPilotOrder.Patrol, in actionParam);
            motive.ConfigurePatrol(center, clampedRadius, clampedSpeed, _arriveDistance, CreateSeed());
        }

        public void Update(ref PilotMotive motive, Vector3 origin)
        {
            switch (motive.Order)
            {
                case EPilotOrder.Patrol:
                    motive.EnsurePatrolAction(origin);
                    break;
            }
        }

        public void OnActionCompleted(ref PilotMotive motive, Vector3 origin)
        {
            switch (motive.Order)
            {
                case EPilotOrder.Patrol:
                    motive.EnsurePatrolAction(origin);
                    break;
            }
        }

        private static uint CreateSeed()
        {
            var value = unchecked((uint)Interlocked.Increment(ref _seedCounter));
            return value == 0u ? 0xA511E9B3u : value;
        }
    }
}
