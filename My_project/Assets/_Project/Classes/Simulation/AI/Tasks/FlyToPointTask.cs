using UnityEngine;

namespace _Project.Scripts.Simulation.AI
{
    /// <summary>Неизменяемая задача метрического полёта к точке.</summary>
    public sealed class FlyToPointTask : ShipAiTask
    {
        public FlyToPointTask(Vector3 destination, float tolerance, bool keepSpeed)
            : base(EShipAiTaskType.FlyToPoint)
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
