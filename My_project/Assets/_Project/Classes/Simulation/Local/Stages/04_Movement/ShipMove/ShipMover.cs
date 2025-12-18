using UnityEngine; // вектора, Mathf, Quaternion
using _Project.Scripts.Ships; // структура Ship
using _Project.Scripts.Simulation.Ships; // ShipTask + параметры

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
        public static Vector3 StepDestinationPosition;

        private static readonly CourseChanger CourseChanger = new();
        private static readonly MoveChanger MoveChanger = new();
        private static readonly SpeedChanger SpeedChanger = new();
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
                ships[i] = ship; // сохраняем изменения
            }
        }

        private static void ProcessShip(ref Ship ship, float deltaTime) //обрабатываем конкретный корабль
        {
            ClearLocalVariables();
            if (!ship.TaskState.TryPeek(out var task) || task.Type != ShipTaskType.MoveToPoint)
                return;

            var moveTaskParams = task.Params.MoveToPointParams;

            CurrPosition = GetCurrentPosition(ship);
            CurrDirection = GetCurrentDirection(ship);
            CurrSpeed = GetCurrentSpeed(ship);

            var toTarget = moveTaskParams.Destination - CurrPosition;// расстояние и направление к цели
            float distance = toTarget.magnitude;
            if (distance <= moveTaskParams.Tolerance)
            {
                CompleteTask(ref ship, ref task, moveTaskParams);
                return;
            }

            NextDirection = CourseChanger.GetDirection(CurrPosition, CurrDirection, moveTaskParams.Destination, ship.Stats.Agility, deltaTime );
            NextSpeed = SpeedChanger.GetSpeed(ref ship, moveTaskParams, deltaTime);
            StepDestinationPosition = GetStepShift(NextDirection, NextSpeed, deltaTime, distance);
            NextPosition = GetNextPosition(CurrPosition, StepDestinationPosition);

            Apply(ref ship);

            if (Vector3.Distance(ship.Position, moveTaskParams.Destination) <= moveTaskParams.Tolerance)
                CompleteTask(ref ship, ref task, moveTaskParams);
        }

        private static void ClearLocalVariables()
        {
            CurrPosition = Vector3.zero;
            CurrDirection = Vector3.zero;
            CurrSpeed = 0;
            NextPosition = Vector3.zero;
            NextDirection = Vector3.zero;
            NextSpeed = 0;
            StepDestinationPosition = Vector3.zero;
        }

        private static Vector3 GetCurrentPosition(in Ship ship)
        {
            return ship.Position;
        }

        private static Vector3 GetCurrentDirection(in Ship ship)
        {
            var forward = ship.Rotation * Vector3.up;
            return forward.sqrMagnitude > 0f ? forward.normalized : Vector3.up;
        }

        private static float GetCurrentSpeed(in Ship ship)
        {
            return Mathf.Max(0f, ship.CurrentSpeed);
        }

        private static Vector3 GetNextPosition(in Vector3 currentPosition, in Vector3 frameShift)
        {
            return currentPosition + frameShift;
        }

        private static Vector3 GetNextDirection(Vector3 currentDirection, Vector3 toTarget, float agility, float deltaTime)
        {
            if (toTarget.sqrMagnitude <= 0f)
                return currentDirection;

            var desired = toTarget.normalized;

            if (currentDirection.sqrMagnitude <= 0f)
                return desired;

            currentDirection.Normalize();
            float maxAngle = Mathf.Max(0f, agility) * Mathf.Rad2Deg * Mathf.Max(0f, deltaTime);
            if (maxAngle <= 0f)
                return desired;

            float angleBetween = Vector3.SignedAngle(currentDirection, desired, Vector3.forward);
            float clampedAngle = Mathf.Clamp(angleBetween, -maxAngle, maxAngle);
            return (Quaternion.AngleAxis(clampedAngle, Vector3.forward) * currentDirection).normalized;
        }

        private static float GetNextSpeed(float currentSpeed, float maxSpeed, float agility, bool keepSpeed, float deltaTime, float distance, float tolerance)
        {
            float targetSpeed;
            if (keepSpeed || agility <= 0f)
            {
                targetSpeed = maxSpeed;
            }
            else
            {
                float required = Mathf.Sqrt(Mathf.Max(0f, 2f * agility * (distance - tolerance)));
                targetSpeed = Mathf.Min(maxSpeed, required);
            }

            return Mathf.MoveTowards(Mathf.Max(0f, currentSpeed), targetSpeed, agility * Mathf.Max(0f, deltaTime));
        }

        private static Vector3 GetStepShift(Vector3 direction, float speed, float deltaTime, float distance)
        {
            if (direction.sqrMagnitude <= 0f || speed <= 0f || deltaTime <= 0f)
                return Vector3.zero;

            float step = Mathf.Min(speed * deltaTime, distance);
            return direction.normalized * step;
        }

        private static void Apply(ref Ship ship)
        {
            ship.Position = NextPosition;
            ship.CurrentSpeed = NextSpeed;
            if (NextDirection.sqrMagnitude > 0f)
                ship.Rotation = Quaternion.LookRotation(Vector3.forward, NextDirection);
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
