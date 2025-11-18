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
    /// <summary>
    /// Обновляет корабли внутри шага: мотивации, поведение, сабстепы.
    /// </summary>
    internal sealed class ShipUpdater
    {
        private readonly RuntimeContext _context;
        private readonly Motivator _motivator;
        private readonly List<ShotEvent> _shotEvents;
        private readonly SubstepTraceBuffer _substeps;

        public ShipUpdater(RuntimeContext context, Motivator motivator, List<ShotEvent> shotEvents, SubstepTraceBuffer substeps)
        {
            _context = context;
            _motivator = motivator;
            _shotEvents = shotEvents;
            _substeps = substeps;
        }

        public void UpdateShips(float dt, int activeSystemIndex)
        {
            if (_context?.Systems == null)
                return;

            _shotEvents.Clear();

            for (int systemId = 0; systemId < _context.Systems.Count; systemId++)
            {
                if (!_context.Systems.TryGetState(systemId, out var state))
                    continue;

                var buffer = state.ShipsBuffer;
                var count = state.ShipCount;

                for (int slot = 0; slot < count; slot++)
                {
                    var ship = buffer[slot];
                    if (!ship.IsActive)
                        continue;

                    if (_context.Pilots == null || !_context.Pilots.TryGetMotiv(ship.PilotUid, out var motiv))
                        continue;

                    _motivator.Update(ref motiv, ship.Position);

                    if (IsActiveSystem(activeSystemIndex, systemId))
                        MoveToPosition.SetTraceWriter(_substeps, in ship.Uid);

                    if (motiv.TryPeekAction(out var action))
                    {
                        var result = ExecuteAction(ref ship, ref motiv, in action, state, dt);
                        if (result.Completed)
                            _motivator.OnActionCompleted(ref motiv, ship.Position);
                    }

                    MoveToPosition.ClearTraceWriter();

                    _context.Systems.TryUpdateShip(systemId, slot, in ship);
                    _context.Pilots.TryUpdateMotiv(ship.PilotUid, in motiv);
                }
            }
        }

        private static bool IsActiveSystem(int activeIndex, int systemId)
        {
            return activeIndex >= 0 && activeIndex == systemId;
        }

        private BehaviorExecutionResult ExecuteAction(ref Ship ship, ref PilotMotive motive, in PilotAction action, StarSystemState state, float dt)
        {
            switch (action.Action)
            {
                case EAction.MoveToCoordinates:
                    return MoveToCoordinatesBehavior.Execute(ref ship, ref motive, in action, dt);
                case EAction.AttackTarget:
                    return AttackTargetBehavior.Execute(ref ship, ref motive, in action, state, dt, _shotEvents);
                case EAction.AcquireTarget:
                    return AcquireTargetBehavior.Execute(ref ship, ref motive, in action, state);
                default:
                    return BehaviorExecutionResult.None;
            }
        }
    }
}
