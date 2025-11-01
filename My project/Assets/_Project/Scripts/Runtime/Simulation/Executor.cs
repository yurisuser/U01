using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Core.Runtime;
using _Project.Scripts.NPC.Fraction;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.Motives;
using sim = _Project.Scripts.Simulation;
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
        private static float DefaultPatrolRadius = PatrolParameters.DefaultPatrolRadius;

        private readonly RuntimeContext _context;
        private readonly GameStateService _state;
        private readonly PilotMotivService _pilotMotivService;

        private bool _initialShipsSpawned;

        public static float DefaultPatrolRadius1 => DefaultPatrolRadius;

        public Executor(RuntimeContext context, GameStateService state)
        {
            _context = context;
            _state = state;
            _pilotMotivService = new PilotMotivService(DefaultPatrolRadius1, ArriveDistance);
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
                        // центрируем патруль вокруг нулевой точки системы (условное расположение звезды)
                        var motiv = _pilotMotivService.CreateDefaultPatrol(Vector3.zero, DefaultPatrolSpeed);
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

                    switch (motiv.ActiveTaskKind)
                    {
                        case PilotTaskKind.PatrolMove:
                            _pilotMotivService.UpdatePatrol(ref ship, ref motiv, dt);
                            break;
                        case PilotTaskKind.None:
                        default:
                            break;
                    }

                    _context.Systems.TryUpdateShip(systemId, slot, in ship);
                    _context.Pilots.TryUpdateMotiv(ship.PilotUid, in motiv);
                }
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
