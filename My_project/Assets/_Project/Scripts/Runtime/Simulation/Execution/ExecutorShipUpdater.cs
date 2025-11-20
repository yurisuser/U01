using System;
using System.Collections.Generic;
using _Project.Scripts.Core;
using _Project.Scripts.Core.Runtime;
using _Project.Scripts.NPC.Fraction;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.Behaviors;
using _Project.Scripts.Simulation.PilotMotivation;
using _Project.Scripts.Simulation.Primitives;
using _Project.Scripts.Simulation.Render;
using UnityEngine;

namespace _Project.Scripts.Simulation.Execution
{
    /// <summary>Обновляет корабли внутри шага: мотивации, поведение, сабстепы.</summary>
    internal sealed class ExecutorShipUpdater
    {
        private readonly RuntimeContext _global_context; // Глобальный контекст со всеми системами и пилотами.
        private readonly Motivator _motivator; // Обновлятор текущей мотивации пилотов.
        private readonly List<ShotEvent> _shotEvents; // Временный список выстрелов за кадр.
        private readonly SubstepTraceBuffer _substeps; // Буфер трассировки перемещений.

        // Сохраняем исходные зависимости для повторного использования во время апдейтов.
        public ExecutorShipUpdater(RuntimeContext context, Motivator motivator, List<ShotEvent> shotEvents, SubstepTraceBuffer substeps)
        {
            _global_context = context;
            _motivator = motivator;
            _shotEvents = shotEvents;
            _substeps = substeps;
        }

        // Главный цикл, проходящий по системам и обновляющий каждый корабль.
        public void UpdateShips(float dt, int activeSystemIndex)
        {
            if (_global_context?.Systems == null)
                return;

            _shotEvents.Clear(); // Сбрасываем выстрелы перед новым циклом.

            for (int systemId = 0; systemId < _global_context.Systems.Count; systemId++) // Обходим все звёздные системы.
            {
                if (!_global_context.Systems.TryGetState(systemId, out var state)) // Пропускаем, если нет состояния системы.
                    continue;

                var buffer = state.ShipsBuffer; // Актуальные корабли.
                var count = state.ShipCount; // Количество активных слотов.

                for (int slot = 0; slot < count; slot++)
                {
                    var ship = buffer[slot]; // Копируем корабль для локальной модификации.
                    if (!ship.IsActive) // Мёртвые корабли пропускаем.
                        continue;

                    if (_global_context.Pilots == null || !_global_context.Pilots.TryGetMotiv(ship.PilotUid, out var motiv)) // Проверяем активного пилота.
                        continue;

                    _motivator.Update(ref motiv, ship.Position); // Обновляем мотивацию относительно позиции.

                    if (IsActiveSystem(activeSystemIndex, systemId)) // В активной системе пишем трассы для визуализации.
                        MoveToPosition.SetTraceWriter(_substeps, in ship.Uid);

                    if (motiv.TryPeekAction(out var action)) // Берём текущую задачу пилота.
                    {
                        var result = ExecuteAction(ref ship, ref motiv, in action, state, dt); // Выполняем поведение.
                        if (result.Completed) // Сообщаем мотиватору об успешном завершении.
                            _motivator.OnActionCompleted(ref motiv, ship.Position);
                    }

                    MoveToPosition.ClearTraceWriter(); // Всегда чистим трассировщик.

                    _global_context.Systems.TryUpdateShip(systemId, slot, in ship); // Возвращаем изменённый корабль.
                    _global_context.Pilots.TryUpdateMotiv(ship.PilotUid, in motiv); // И обновлённую мотивацию пилота.
                }
            }
        }

        // Проверяем, является ли система активной для детального трейсинга.
        private static bool IsActiveSystem(int activeIndex, int systemId)
        {
            return activeIndex >= 0 && activeIndex == systemId;
        }

        // Подбираем и запускаем поведение в зависимости от текущего действия пилота.
        private BehaviorExecutionResult ExecuteAction(ref Ship ship, ref PilotMotive motive, in PilotAction action, StarSystemState state, float dt)
        {
            switch (action.Action)
            {
                case EAction.MoveToCoordinates:
                    return MoveToCoordinatesBehavior.Execute(ref ship, ref motive, in action, dt);
                case EAction.AttackTarget:
                    return AttackTargetBehavior.Execute(ref ship, ref motive, in action, state, dt, _shotEvents);
                case EAction.AcquireTarget:
                    return ChoiceTargetBehavior.Execute(ref ship, ref motive, in action, state);
                default:
                    return BehaviorExecutionResult.None;
            }
        }
    }
}
