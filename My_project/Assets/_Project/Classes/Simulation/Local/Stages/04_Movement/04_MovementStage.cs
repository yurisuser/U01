using UnityEngine;
using _Project.Scripts.Simulation.Ships;

namespace _Project.Scripts.Simulation.Local.Stages
{
    /// <summary>Движение юнитов/снарядов.</summary>
    public sealed class LocalMovementStage : ILocalSimulationStage
    {
        public void Run(in LocalSimulationContext context)
        {
            if (!context.HasActiveSystem)
                return;

            var system = context.ActiveSystem.Value;
            var runtime = system.State;
            if (runtime == null)
                return;

            var ships = runtime.Ships;
            float delta = Mathf.Max(0f, context.DeltaTime);

            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                if (!ship.TaskState.TryPeek(out var task))
                    continue;

                if (task.Type == ShipTaskType.MoveToPoint)
                {
                    var move = task.Params.MoveToPoint;
                    float speed = Mathf.Max(0f, ship.Stats.MaxSpeed);
                    ship.Position = Vector3.MoveTowards(ship.Position, move.Target, speed * delta);

                    if (Vector3.Distance(ship.Position, move.Target) <= move.Tolerance)
                        ship.TaskState.Pop();
                }

                ships[i] = ship;
            }
        }
    }
}
