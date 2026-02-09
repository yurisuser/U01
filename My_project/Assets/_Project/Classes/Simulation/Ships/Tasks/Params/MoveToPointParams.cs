using UnityEngine;

namespace _Project.Scripts.Simulation.Ships
{
    public struct MoveToPointParams
    {
        public Vector3 Destination;
        public float Tolerance;
        public bool KeepSpeed;
        public _Project.Scripts.Core.UID TargetUid;
    }
}
