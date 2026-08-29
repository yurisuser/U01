using _Project.Scripts.Const;
using _Project.Scripts.Ships;
using _Project.Scripts.Ships.Orders;

namespace _Project.Scripts.Simulation.AI
{
    /// <summary>Временно переводит поддержанный старый приказ в новый каркас ИИ.</summary>
    public static class ShipAiLegacyOrderBridge
    {
        public static void TryMigratePatrol(ref Ship ship)
        {
            if (ship.Ai == null || !ship.Ai.CurrentOrder.IsEmpty || ship.TopOrder.Type != ETopShipOrderType.Patrol)
                return;

            ShipAiOrders.Patrol(
                ref ship,
                ship.TopOrder.Params.Center,
                ship.TopOrder.Params.Radius,
                SimulationConsts.DestinationPointTolerance);
        }
    }
}
