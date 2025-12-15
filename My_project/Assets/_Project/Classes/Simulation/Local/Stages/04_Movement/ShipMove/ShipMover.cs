using UnityEngine; // вектора, Mathf, Quaternion
using _Project.Scripts.Ships; // структура Ship
using _Project.Scripts.Simulation.Ships; // ShipTask + параметры

namespace _Project.Scripts.Simulation.Local.Stages.Movement // пространство имён стадии движения
{
    /// <summary>Оркестратор перемещения кораблей: направление → скорость → шаг.</summary>
    public sealed class ShipMover : IMovementProcessor // имплементируем интерфейс движения
    {
        public void Run(in LocalSimulationContext context) // основной вход стадии
        {
            var system = context.ActiveSystem.Value;
            var runtime = system.State;
            if (runtime == null)
                return; // выходим

            var ships = runtime.Ships;
            float delta = Mathf.Max(0f, context.DeltaTime);

            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                ProcessShip(ref ship, delta);
                ships[i] = ship;
            }
        }

        private static void ProcessShip(ref Ship ship, float deltaTime)
        {
            if (!ship.TaskState.TryPeek(out var task))
                return;

            if (task.Type != ShipTaskType.MoveToPoint)
                return;

            var move = task.Params.MoveToPoint;
            var toTarget = move.Target - ship.Position;
            float distance = toTarget.magnitude;

            if (distance <= move.Tolerance)
            {
                CompleteTask(ref ship, ref task, move);
                return;
            }

            var direction = toTarget / distance;
            float step = Mathf.Min(ship.Stats.MaxSpeed * deltaTime, distance);
            ship.Position += direction * step;

            if (Vector3.Distance(ship.Position, move.Target) <= move.Tolerance)
                CompleteTask(ref ship, ref task, move);
        }

        private static void CompleteTask(ref Ship ship, ref ShipTask task, in MoveToPointParams move) // финализация MoveToPoint
        {
            ship.Position = move.Target; // фиксируемся точно в целевой точке
            if (!move.KeepSpeed) // если требуется остановка
                ship.CurrentSpeed = 0f; // обнуляем скорость
            ship.TaskState.Pop(); // удаляем задачу из стека
        }
    }
}
