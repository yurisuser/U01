using System.Collections.Generic;
using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Core.Runtime;
using _Project.Scripts.NPC.Fraction;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.Behaviors;
using _Project.Scripts.Simulation.PilotMotivation;
using _Project.Scripts.Simulation.Primitives;
using UnityEngine;

namespace _Project.Scripts.Simulation
{
    /// <summary>
    /// Исполняет игровой шаг: обновляет задачи, перемещает корабли и синхронизирует снапшот UI.
    /// </summary>
    public sealed class Executor
    {
        private const int ShipsPerSystem = 5;
        private const float SpawnRadius = 6f;
        private const float ArriveDistance = 0.2f; // расстояние, с которого патруль считается достигшим цели и берёт новую точку маршрута
        private const float DefaultPatrolSpeed = 5f;
        private static float DefaultPatrolRadius = 200f;

        private readonly RuntimeContext _context;
        private readonly GameStateService _state;
        private readonly Motivator _motivator;
        private readonly List<ShotEvent> _shotEvents = new List<ShotEvent>(64);

        private bool _initialShipsSpawned;

        public static float DefaultPatrolRadius1 => DefaultPatrolRadius;

        public Executor(RuntimeContext context, GameStateService state)
        {
            _context = context;
            _state = state;
            _motivator = new Motivator(DefaultPatrolRadius1, ArriveDistance, DefaultPatrolSpeed);
        }

        public void Execute(ref GameStateService.Snapshot snapshot, float dt)
        {
            EnsureInitialShips();

            if (_context != null)
            {
                _context.Tasks.Tick(dt);
                _context.Ships.Tick(dt);
                UpdateShips(dt);
            }

            DoLogicStep(ref snapshot, dt);
            _state?.MarkDynamicDirty();
        }

        private void EnsureInitialShips()
        {
            if (_initialShipsSpawned || _context == null)
                return;

            var galaxyCount = _context.Galaxy?.Count ?? 0;
            if (galaxyCount <= 0)
                return;

            for (int systemId = 0; systemId < galaxyCount; systemId++)
            {
                for (int i = 0; i < ShipsPerSystem; i++)
                {
                    var faction = PickFactionForSpawn(systemId, i);

                    var pilotUid = UIDService.Create(EntityType.Individ);
                    var ship = ShipCreator.CreateShip(faction, pilotUid);
                    EquipmentGenerator.InitForShip(ref ship);

                    // Добавляем разброс скоростей +-10% от базовой
                    var stats = ship.Stats;
                    if (stats.MaxSpeed > 0f)
                    {
                        float jitter = Rng.Range(-0.1f, 0.1f);
                        float factor = 1f + jitter;
                        stats.MaxSpeed = Mathf.Max(0.1f, stats.MaxSpeed * factor);
                        ship.Stats = stats;
                    }

                    float angle = i / (float)ShipsPerSystem * Mathf.PI * 2f;
                    float edgeRadius = SpawnRadius * 20f;
                    ship.Position = new Vector3(
                        Mathf.Cos(angle) * edgeRadius,
                        Mathf.Sin(angle) * edgeRadius,
                        0f);
                    ship.Rotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
                    ship.IsActive = true;

                    _context.Ships.RegisterShip(systemId, ship);

                    if (_context.Pilots != null)
                    {
                        float searchRadius = Mathf.Max(SpawnRadius * 10f, 250f);
                        var motiv = _motivator.CreateAttackAll(searchRadius, allowFriendlyFire: false);
                        _context.Pilots.SetMotiv(pilotUid, in motiv);
                    }
                }
            }

            _initialShipsSpawned = true;
        }

        public IReadOnlyList<ShotEvent> ShotEvents => _shotEvents;

        private void UpdateShips(float dt)
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

                    if (motiv.TryPeekAction(out var action))
                    {
                        var result = ExecuteAction(ref ship, ref motiv, in action, state, dt);
                        if (result.Completed)
                            _motivator.OnActionCompleted(ref motiv, ship.Position);
                    }

                    _context.Systems.TryUpdateShip(systemId, slot, in ship);
                    _context.Pilots.TryUpdateMotiv(ship.PilotUid, in motiv);
                }
            }
        }

        private static Fraction PickFactionForSpawn(int systemId, int shipIndex)
        {
            var fractions = Fractions.All;
            if (fractions == null || fractions.Length == 0)
                return new Fraction(EFraction.fraction1, "Default");

            uint state = unchecked((uint)((systemId + 1) * 73856093) ^ (uint)((shipIndex + 1) * 19349663));
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;

            var idx = (int)(state % (uint)fractions.Length);
            return fractions[idx];
        }

        private static void DoLogicStep(ref GameStateService.Snapshot snapshot, float dt)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"Logic tick: {snapshot.TickIndex}, dt={dt:0.###}");
#endif
        }

        private BehaviorExecutionResult ExecuteAction(
            ref Ship ship,
            ref PilotMotive motive,
            in PilotAction action,
            StarSystemState state,
            float dt)
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
