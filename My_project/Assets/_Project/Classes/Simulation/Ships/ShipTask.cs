using UnityEngine;

namespace _Project.Scripts.Simulation.Ships
{
    public struct ShipTask
    {
        public ShipTask(ShipTaskType type, ShipTaskParams parameters)
        {
            Type = type;
            Params = parameters;
        }

        public ShipTaskType Type;
        public ShipTaskParams Params;

        public static ShipTask MoveTo(Vector3 target, float tolerance, bool keepSpeed = false)
        {
            return new ShipTask(ShipTaskType.MoveToPoint, new ShipTaskParams
            {
                Type = ShipTaskType.MoveToPoint,
                MoveToPoint = new MoveToPointParams
                {
                    Target = target,
                    Tolerance = tolerance,
                    KeepSpeed = keepSpeed
                }
            });
        }
    }
}
