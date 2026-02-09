using UnityEngine;
using _Project.Scripts.Ships;
using _Project.Scripts.Ships.Orders;

namespace _Project.Scripts.Simulation.Ships
{
    /// <summary>Единое правило назначения базового межсистемного trade-order для ship.</summary>
    internal static class TradeTopOrderAssigner
    {
        public static bool EnsureTradeGalaxyOrder(ref Ship ship, int systemIndex, float radius)
        {
            if (ship.TopOrder.Type == ETopShipOrderType.TradeGalaxy)
                return false; // Уже в целевом режиме.

            if (!ship.TopOrder.IsEmpty && ship.TopOrder.Type != ETopShipOrderType.TradeInSystem)
                return false; // Чужой приказ не перетираем.

            ship.TopOrder = new TopShipOrder
            {
                Type = ETopShipOrderType.TradeGalaxy,
                Params = new TopShipOrderParams
                {
                    Center = Vector3.zero,
                    Radius = radius,
                    SystemIndex = systemIndex,
                }
            };

            return true;
        }
    }
}
