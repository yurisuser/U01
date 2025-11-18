using System.Collections.Generic;
using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Core.Runtime;
using _Project.Scripts.NPC.Fraction;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.Behaviors;
using _Project.Scripts.Simulation.PilotMotivation;
using _Project.Scripts.Simulation.Render;
using _Project.Scripts.Simulation;
using UnityEngine;

namespace _Project.Scripts.Simulation.Execution
{
    /// <summary>
    /// Исполняет игровой шаг: обновляет задачи, перемещает корабли и синхронизирует снапшот UI.
    /// </summary>
    public sealed class Executor
    {
        private readonly RuntimeContext _context;                     // контекст мира
        private readonly GameStateService _state;                     // сервис состояния
        private readonly Motivator _motivator;                        // конфигурация мотиваций
        private readonly ShipUpdater _shipUpdater;                    // сервис обновления кораблей
        private readonly Spawn.ShipSpawnService _shipSpawner;         // сервис первичного спавна
        private readonly List<ShotEvent> _shotEvents = new List<ShotEvent>(64); // общий буфер событий выстрелов
        private readonly Render.SubstepTraceBuffer _substeps = new Render.SubstepTraceBuffer(); // буфер сабстепов

        public Executor(RuntimeContext context, GameStateService state)
        {
            _context = context;
            _state = state;
            _motivator = new Motivator(SimulationConsts.DefaultPatrolRadius, SimulationConsts.ArriveDistance, SimulationConsts.DefaultPatrolSpeed);
            _shipUpdater = new ShipUpdater(_context, _motivator, _shotEvents, _substeps);
            _shipSpawner = new Spawn.ShipSpawnService(_context, _motivator);
        }

        public void Execute(ref GameStateService.Snapshot snapshot, float dt)
        {
            _shipSpawner.EnsureInitialShips();
            _substeps.BeginTick();

            if (_context != null)
            {
                _context.Tasks.Tick(dt);
                _context.Ships.Tick(dt);
                _shipUpdater.UpdateShips(dt, snapshot.SelectedSystemIndex);
            }

            DoLogicStep(ref snapshot, dt);
            _state?.MarkDynamicDirty();
            _substeps.Publish();
            _state?.SetSubstepTraces(_substeps.Published, snapshot.SelectedSystemIndex);
        }

        public IReadOnlyList<ShotEvent> ShotEvents => _shotEvents;

        private static void DoLogicStep(ref GameStateService.Snapshot snapshot, float dt)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"Logic tick: {snapshot.TickIndex}, dt={dt:0.###}");
#endif
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
