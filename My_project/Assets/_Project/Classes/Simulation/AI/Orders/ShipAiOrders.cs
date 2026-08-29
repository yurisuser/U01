using UnityEngine;
using _Project.Scripts.Ships;

namespace _Project.Scripts.Simulation.AI
{
    /// <summary>Точка назначения явных приказов новому ИИ корабля.</summary>
    public static class ShipAiOrders
    {
        public static void MoveToPoint(ref Ship ship, Vector3 destination, float tolerance, bool keepSpeed = false)
        {
            ship.Ai.ReplaceOrder(ShipAiOrder.MoveToPoint(destination, tolerance, keepSpeed));
        }

        public static void Patrol(ref Ship ship, Vector3 center, float radius, float tolerance)
        {
            ship.Ai.ReplaceOrder(ShipAiOrder.Patrol(center, radius, tolerance));
        }

        public static void TradeInSystem(ref Ship ship, float tolerance)
        {
            ship.Ai.ReplaceOrder(ShipAiOrder.TradeInSystem(tolerance));
        }
    }
}
