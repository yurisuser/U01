using UnityEngine;

namespace _Project.Scripts.Simulation.AI
{
    /// <summary>Корневое поведение простого приказа движения.</summary>
    public sealed class MoveToPointBehavior : ShipAiBehavior
    {
        private readonly Vector3 _destination;
        private readonly float _tolerance;
        private readonly bool _keepSpeed;
        private bool _taskCreated;

        public MoveToPointBehavior(Vector3 destination, float tolerance, bool keepSpeed)
        {
            _destination = destination;
            _tolerance = tolerance;
            _keepSpeed = keepSpeed;
        }

        public override bool TryCreateTask(in _Project.Scripts.Ships.Ship ship, in _Project.Scripts.Galaxy.Data.StarSys system, out ShipAiTask task)
        {
            if (_taskCreated)
            {
                task = null;
                return false;
            }

            _taskCreated = true;
            task = new MoveToPointTask(_destination, _tolerance, _keepSpeed);
            return true;
        }
    }
}
