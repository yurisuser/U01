using UnityEngine;
using _Project.Scripts.Const;
using _Project.Scripts.Ships;

namespace _Project.Scripts.Simulation.AI
{
    /// <summary>Временное назначение общего приказа до появления базовых назначений.</summary>
    public static class ShipInitialOrderAssigner
    {
        public static void EnsureOrder(ref Ship ship, int systemIndex, float radius)
        {
            if (ship.Ai.CurrentOrder.Type == EShipAiOrderType.TradeInSystem)
                return;

            ship.TopOrder = default;
            ShipAiOrders.TradeInSystem(ref ship, SimulationConsts.DestinationPointTolerance);
        }
    }
}
