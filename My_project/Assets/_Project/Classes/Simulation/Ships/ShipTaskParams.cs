using UnityEngine;

namespace _Project.Scripts.Simulation.Ships
{
    public struct ShipTaskParams
    {
        public ShipTaskType TypeTask;
        public MoveToPointParams MoveToPointParams;
    }

    public struct MoveToPointParams
    {
        public Vector3 Destination;
        public float Tolerance;
        public bool KeepSpeed;
    }
}
