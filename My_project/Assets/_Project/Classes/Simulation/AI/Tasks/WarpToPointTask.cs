using UnityEngine;

namespace _Project.Scripts.Simulation.AI
{
    /// <summary>Запрос подготовки и выполнения варпа; завершается после возвращения манёвров.</summary>
    public sealed class WarpToPointTask : ShipAiTask
    {
        public Vector3 Destination { get; }
        public float ArrivalTolerance { get; }

        public WarpToPointTask(Vector3 destination, float arrivalTolerance) : base(EShipAiTaskType.WarpToPoint)
        {
            Destination = destination;
            ArrivalTolerance = arrivalTolerance;
        }
    }
}
