using UnityEngine;

namespace _Project.Scripts.Simulation.Ships
{
    public struct ShipTaskParams
    {
        public ShipTaskType Type;
        public MoveToPointParams MoveToPoint;
    }

    public struct MoveToPointParams
    {
        public Vector3 Target;
        public float Tolerance;
        public bool KeepSpeed;
    }
}
