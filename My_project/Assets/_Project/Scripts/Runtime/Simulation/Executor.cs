using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Core.Runtime;
using _Project.Scripts.NPC.Fraction;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.PilotMotivation;
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
        private static float DefaultPatrolRadius = 100f;

        private readonly RuntimeContext _context;
        private readonly GameStateService _state;
        private readonly Motivator _motivator;

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
                    var faction = Fractions.All.Length > 0
                        ? Fractions.All[(systemId + i) % Fractions.All.Length]
                        : new Fraction(EFraction.fraction1, "Default");

                    var pilotUid = UIDService.Create(EntityType.Individ);
                    var ship = ShipCreator.CreateShip(faction, pilotUid);

                    float angle = i / (float)ShipsPerSystem * Mathf.PI * 2f;
                    ship.Position = new Vector3(
                        Mathf.Cos(angle) * SpawnRadius,
                        Mathf.Sin(angle) * SpawnRadius,
                        0f);
                    ship.Rotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
                    ship.IsActive = true;

                    _context.Ships.RegisterShip(systemId, ship);

                    if (_context.Pilots != null)
                    {
                        float patrolSpeed = ship.MaxSpeed > 0f ? ship.MaxSpeed * 0.5f : ship.MaxSpeed;
                        var motiv = _motivator.CreateDefaultPatrol(ship.Position, patrolSpeed);
                        _context.Pilots.SetMotiv(pilotUid, in motiv);
                    }
                }
            }

            _initialShipsSpawned = true;
        }

        private void UpdateShips(float dt)
        {
            if (_context?.Systems == null)
                return;

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
                        switch (action.Action)
                        {
                            case EAction.MoveToCoordinates:
                                ExecuteMove(ref ship, ref motiv, in action, dt);
                                break;
                            default:
                                break;
                        }
                    }

                    _context.Systems.TryUpdateShip(systemId, slot, in ship);
                    _context.Pilots.TryUpdateMotiv(ship.PilotUid, in motiv);
                }
            }
        }

        private void ExecuteMove(ref Ship ship, ref PilotMotive motive, in PilotAction action, float dt)
        {
            var move = action.Parameters.Move;
            var target = move.Destination;
            var arriveDistance = Mathf.Max(move.ArriveDistance, 0.01f);
            var toTarget = target - ship.Position;
            var distance = toTarget.magnitude;

            if (distance <= arriveDistance)
            {
                ship.Position = target;
                motive.CompleteCurrentAction();
                _motivator.OnActionCompleted(ref motive, ship.Position);
                return;
            }

            var desiredSpeed = Mathf.Max(move.DesiredSpeed, 0.1f);
            if (ship.MaxSpeed > 0f)
                desiredSpeed = Mathf.Min(desiredSpeed, ship.MaxSpeed);

            var forward = ship.Rotation * Vector3.right;
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.right;
            forward.Normalize();

            float turnRadius = ship.Agility > 0f ? 1f / ship.Agility : float.PositiveInfinity;

            const float MaxSubstep = 0.1f;
            int steps = Mathf.Clamp(Mathf.CeilToInt(dt / MaxSubstep), 1, 60);
            float subDt = dt / steps;
            bool reachedTarget = false;

            for (int i = 0; i < steps; i++)
            {
                toTarget = target - ship.Position;
                distance = toTarget.magnitude;

                if (distance <= arriveDistance)
                {
                    ship.Position = target;
                    reachedTarget = true;
                    break;
                }

                var desiredDir = distance > Mathf.Epsilon ? toTarget / distance : Vector3.zero;

                if (desiredDir.sqrMagnitude > Mathf.Epsilon && !float.IsInfinity(turnRadius))
                {
                    float maxTurnRate = desiredSpeed / Mathf.Max(turnRadius, 0.0001f);
                    float maxTurn = maxTurnRate * subDt;
                    forward = Vector3.RotateTowards(forward, desiredDir, maxTurn, 0f).normalized;
                }

                float subDistance = desiredSpeed * subDt;

                if (!float.IsInfinity(turnRadius) && desiredDir.sqrMagnitude > Mathf.Epsilon)
                {
                    float distanceAlongForward = Vector3.Dot(toTarget, forward);
                    if (distanceAlongForward <= 0f)
                    {
                        continue;
                    }

                    if (subDistance > distanceAlongForward)
                        subDistance = distanceAlongForward;
                }

                ship.Position += forward * subDistance;
            }

            if (forward.sqrMagnitude > Mathf.Epsilon)
            {
                float angleDeg = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
                ship.Rotation = Quaternion.Euler(0f, 0f, angleDeg);
            }

            if (reachedTarget || (target - ship.Position).sqrMagnitude <= arriveDistance * arriveDistance)
            {
                ship.Position = target;
                motive.CompleteCurrentAction();
                _motivator.OnActionCompleted(ref motive, ship.Position);
            }
        }

        private static void DoLogicStep(ref GameStateService.Snapshot snapshot, float dt)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"Logic tick: {snapshot.TickIndex}, dt={dt:0.###}");
#endif
        }
    }
}
