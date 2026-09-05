using UnityEngine;
using _Project.Scripts.Simulation.Ships.Movement;

namespace _Project.Scripts.Simulation.Local.Stages.Movement
{
    /// <summary>Оркестратор движения: прыжок в континуум или шаг локального перемещения.</summary>
    public sealed class ShipMover : IMovementProcessor
    {
        private readonly ShipJumpProcessor _jumpProcessor = new(); // Перенос ship в continuum.
        private readonly ShipMoveTaskProcessor _moveTaskProcessor = new(new CourseChanger(), new MoveChanger(), new SpeedChanger()); // Шаг локального MoveToPoint.

        public void Run(in LocalSimulationContext context)
        {
            var system = context.ActiveSystem.Value;
            var runtime = system.State;
            if (runtime == null)
                return; // Без runtime нет динамики кораблей.

            var ships = runtime.Ships;
            float delta = Mathf.Max(0f, context.DeltaTime);

            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                if (ShipWarpProcessor.Process(ref ship, delta, context.IsTurnStart))
                {
                    ships[i] = ship; // Активная фаза варпа полностью владеет движением корабля.
                    continue;
                }

                if (_jumpProcessor.TryProcessJump(ref ship, in context, ships, i))
                {
                    i--; // RemoveAt сдвинул хвост списка.
                    continue;
                }

                _moveTaskProcessor.ProcessMove(ref ship, delta); // Выполняем один дискретный шаг движения.
                ships[i] = ship; // value-type: сохраняем изменения структуры.
            }
        }
    }
}
