using UnityEngine; // вектора, Mathf, Quaternion
using _Project.Scripts.Ships; // структура Ship
using _Project.Scripts.Simulation.Ships;
using System.Threading.Tasks; // ShipTask + параметры

namespace _Project.Scripts.Simulation.Local.Stages.Movement // пространство имён стадии движения
{
    /// <summary>Оркестратор перемещения кораблей: направление → скорость → шаг.</summary>
    public sealed class ShipMover : IMovementProcessor // имплементируем интерфейс движения
    {
        public static Vector3 CurrPosition;
        public static Vector3 CurrDirection;
        public static float CurrSpeed;
        public static Vector3 NextPosition;
        public static Vector3 NextDirection;
        public static float NextSpeed;

        public void Run(in LocalSimulationContext context) // основной вход стадии
        {
            var runtime = context.ActiveSystem.Value.State;
            if (runtime == null)
                return; // выходим

            var ships = runtime.Ships;
            float delta = Mathf.Max(0f, context.DeltaTime);

            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                ProcessShip(ref ship, delta);
                ships[i] = ship; // сохраняем изменения
            }
        }

        private static void ProcessShip(ref Ship ship, float deltaTime) //обрабатываем конкретный корабль
        {
            if (!ship.TaskState.TryPeek(out var task) || task.Type != ShipTaskType.MoveToPoint ) return;

            CurrPosition = GetCurrentPosition(ship);
            CurrDirection = GetCurrentDirection(ship);
            CurrSpeed = GetCurrentSpeed(ship);
            NextDirection = GetNextDirection(ship);
            NextSpeed = GetNextSpeed(ship);
            NextDirection = GetNextDirection(ship);
            NextPosition = GetNextPosition(ship);

            var taskParams = task.Params.MoveToPointParams;
            var full_direction = taskParams.Destination - ship.Position; //del
            float distanceToDest = full_direction.magnitude; //del
            //проверка, а вдруг уже прилетели
            if (distanceToDest <= taskParams.Tolerance)
            {
                CompleteTask(ref ship, ref task, taskParams);
                return;
            }
            //Расчеты
            var normal_direction = full_direction / distanceToDest;
            float wayInFrame = Mathf.Min(ship.Stats.MaxSpeed * deltaTime, distanceToDest);
            ship.Position += normal_direction * wayInFrame;
            //если долетим за этот ход
            if (Vector3.Distance(ship.Position, taskParams.Destination) <= taskParams.Tolerance)
                CompleteTask(ref ship, ref task, taskParams);
        }

        private static void ClearLocalVariables()
        {
            CurrPosition = Vector3.zero;
            CurrDirection = Vector3.zero;
            CurrSpeed = 0;
            NextPosition = Vector3.zero;
            NextDirection = Vector3.zero;
            NextSpeed = 0;
        }

        //TODO проверить, если корабль будет вечно вращаться, то остановиться и повернуться
        
        private static Vector3 GetCurrentPosition(in Ship ship)
        {
            return Vector3.zero;
        }

        private static Vector3 GetCurrentDirection(in Ship ship)
        {
            return Vector3.zero;
        }

        private static float GetCurrentSpeed(in Ship ship)
        {
            return 0f;
        }

        private static Vector3 GetNextPosition(in Ship ship)
        {
            return Vector3.zero;
        }

        private static Vector3 GetNextDirection(in Ship ship)
        {
            return Vector3.zero;
        }

        private static float GetNextSpeed(in Ship ship)
        {
            return 0f;
        }

        private static void CompleteTask(ref Ship ship, ref ShipTask task, in MoveToPointParams move) // финализация MoveToPoint
        {
            ship.Position = move.Destination; // фиксируемся точно в целевой точке
            if (!move.KeepSpeed) // если требуется остановка
                ship.CurrentSpeed = 0f; // обнуляем скорость
            ship.TaskState.Pop(); // удаляем задачу из стека
        }
    }
}
