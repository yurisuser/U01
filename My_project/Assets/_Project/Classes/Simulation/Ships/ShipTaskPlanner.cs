using UnityEngine;
using _Project.Scripts.Const;
using _Project.Scripts.Ships;
using _Project.Scripts.Ships.Orders;

namespace _Project.Scripts.Simulation.Ships
{
    public static class ShipTaskPlanner
    {
        public static void EnsurePatrolTask(ref Ship ship)
        {
            if (ship.TopOrder.Type != ETopShipOrderType.Patrol)
                return;

            if (ship.TaskState.HasTasks)
                return;

            float radius = ship.TopOrder.Params.Radius;
            var target = SamplePatrolPoint(radius, ship.TopOrder.Params.Center);
            ship.TaskState.PushTask(ShipTaskBuilder.MoveTo(target, SimulationConsts.DestinationPointTolerance, keepSpeed: true));
        }

        private static Vector3 SamplePatrolPoint(float radius, Vector3 center)
        {
            var inside = Random.insideUnitCircle * radius;
            return new Vector3(center.x + inside.x, center.y + inside.y, 0f);
        }
    }
}
