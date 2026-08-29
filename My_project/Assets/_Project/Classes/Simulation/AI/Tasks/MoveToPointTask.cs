using UnityEngine;

namespace _Project.Scripts.Simulation.AI
{
    /// <summary>Задача перемещения к заданной точке.</summary>
    public sealed class MoveToPointTask : ShipAiTask
    {
        public MoveToPointTask(Vector3 destination, float tolerance, bool keepSpeed)
            : base(EShipAiTaskType.MoveToPoint)
        {
            Destination = destination;
            Tolerance = tolerance;
            KeepSpeed = keepSpeed;
        }

        public Vector3 Destination { get; }
        public float Tolerance { get; }
        public bool KeepSpeed { get; }
    }
}
