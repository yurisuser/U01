using System.Collections.Generic;
using UnityEngine;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.Continuum;
using _Project.Scripts.Simulation.Ships;

namespace _Project.Scripts.Simulation.Local.Stages.Movement
{
    /// <summary>Обработчик задачи JumpToSystem: удаление из системы и постановка транзита.</summary>
    internal sealed class ShipJumpProcessor
    {
        public bool TryProcessJump(ref Ship ship, in LocalSimulationContext context, List<Ship> ships, int index)
        {
            if (!ship.TaskState.TryPeek(out var task) || task.Type != EShipTaskType.JumpToSystem)
                return false; // Верхняя задача не jump.

            var service = ContinuumService.Instance;
            var gameState = context.GameState;
            if (service == null || gameState == null)
                return false; // Нет сервиса или состояния мира.

            int fromIndex = gameState.ActiveLocalSystemIndex;
            int toIndex = task.Params.JumpToSystemParams.TargetSystemIndex;
            var galaxy = gameState.Galaxy;
            if (galaxy == null || fromIndex < 0 || fromIndex >= galaxy.Length || toIndex < 0 || toIndex >= galaxy.Length)
                return false; // Некорректные индексы систем.

            if (!service.TryGetZone(fromIndex, toIndex, out var zone))
                return false; // Нет зоны перехода между системами.

            float distance = Vector3.Distance(ship.Position, zone.Center);
            if (distance > zone.Radius)
                return false; // Корабль еще не вошел в зону jump.

            ships.RemoveAt(index); // Убираем корабль из локального списка системы.

            var transit = service.CreateTransit(ship, fromIndex, toIndex, galaxy);
            service.Enqueue(in transit); // Передаем владение кораблем континууму.

            ship.TaskState.Pop(); // Jump-задача выполнена.
            return true;
        }
    }
}
