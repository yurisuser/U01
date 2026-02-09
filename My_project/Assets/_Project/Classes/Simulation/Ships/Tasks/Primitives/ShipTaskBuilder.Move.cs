using UnityEngine;

namespace _Project.Scripts.Simulation.Ships
{
    public static partial class ShipTaskBuilder
    {
        public static ShipTask MoveToPosition(
            Vector3 target,
            float tolerance,
            bool keepSpeed = false,
            _Project.Scripts.Core.UID targetUid = default)
        {
            return MoveTo(target, tolerance, keepSpeed, targetUid);
        }

        public static ShipTask MoveTo(
            Vector3 target,
            float tolerance,
            bool keepSpeed = false,
            _Project.Scripts.Core.UID targetUid = default)
        {
            return new ShipTask(EShipTaskType.MoveToPoint, new ShipTaskParams
            {
                TypeTask = EShipTaskType.MoveToPoint,
                MoveToPointParams = new MoveToPointParams
                {
                    Destination = target,
                    Tolerance = tolerance,
                    KeepSpeed = keepSpeed,
                    TargetUid = targetUid
                }
            });
        }
    }
}
