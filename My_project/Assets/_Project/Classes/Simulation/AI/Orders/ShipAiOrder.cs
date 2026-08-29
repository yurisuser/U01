using UnityEngine;

namespace _Project.Scripts.Simulation.AI
{
    /// <summary>Текущий приказ нового ИИ корабля.</summary>
    public struct ShipAiOrder
    {
        public EShipAiOrderType Type;
        public Vector3 Destination;
        public float Tolerance;
        public bool KeepSpeed;
        public Vector3 Center;
        public float Radius;

        public bool IsEmpty => Type == EShipAiOrderType.None;

        public static ShipAiOrder MoveToPoint(Vector3 destination, float tolerance, bool keepSpeed = false)
        {
            return new ShipAiOrder
            {
                Type = EShipAiOrderType.MoveToPoint,
                Destination = destination,
                Tolerance = tolerance,
                KeepSpeed = keepSpeed,
            };
        }

        public static ShipAiOrder Patrol(Vector3 center, float radius, float tolerance)
        {
            return new ShipAiOrder
            {
                Type = EShipAiOrderType.Patrol,
                Center = center,
                Radius = radius,
                Tolerance = tolerance,
                KeepSpeed = true,
            };
        }

        public static ShipAiOrder TradeInSystem(float tolerance)
        {
            return new ShipAiOrder
            {
                Type = EShipAiOrderType.TradeInSystem,
                Tolerance = tolerance,
            };
        }
    }
}
