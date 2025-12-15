using UnityEngine;
using _Project.Scripts.Ships;

namespace _Project.Scripts.Simulation.Ships
{
    public static class ShipTaskPlanner
    {
        public static void EnsurePatrolTask(ref Ship ship, float patrolRadius)
        {
            if (ship.TaskState.HasTasks)
                return;

            var target = SamplePatrolPoint(patrolRadius);
            ship.TaskState.PushTask(ShipTask.MoveTo(target, 2f));
        }

        private static Vector3 SamplePatrolPoint(float radius)
        {
            var inside = Random.insideUnitCircle * radius;
            return new Vector3(inside.x, inside.y, 0f);
        }
    }
}
