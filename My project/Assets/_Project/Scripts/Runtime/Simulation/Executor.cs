using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Core.Runtime;
using _Project.Scripts.NPC.Fraction;
using _Project.Scripts.Ships;
using UnityEngine;

namespace _Project.Scripts.Simulation
{
    /// <summary>
    /// Выполняет игровой шаг: следит за задачами, обновляет корабли и сообщает UI об изменениях.
    /// </summary>
    public sealed class Executor
    {
        private const int ShipsPerSystem = 3;   // сколько кораблей создаём на старте в каждой системе
        private const float SpawnRadius = 6f;   // радиус размещения кораблей вокруг центра системы
        private const float BaseAngularSpeed = 0.6f; // базовая угловая скорость патруля (рад/с)

        private readonly RuntimeContext _context;
        private readonly GameStateService _state;

        private bool _initialShipsSpawned;

        public Executor(RuntimeContext context, GameStateService state)
        {
            _context = context;
            _state = state;
        }

        public void Execute(ref GameStateService.Snapshot snapshot, float dt)
        {
            EnsureInitialShips();

            if (_context != null)
            {
                _context.Tasks.Tick(dt);
                _context.Ships.Tick(dt); // зарезервировано для будущей логики флотов
                UpdateShips(dt);
            }

            DoLogicStep(ref snapshot, dt);

            // Сообщаем UI, что динамика обновилась (позиции кораблей и пр.)
            _state?.RefreshDynamicSnapshot();
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

                    // Разбрасываем корабли по окружности, чтобы они не слипались
                    float angle = i / (float)ShipsPerSystem * Mathf.PI * 2f;
                    ship.Position = new Vector3(
                        Mathf.Cos(angle) * SpawnRadius,
                        Mathf.Sin(angle) * SpawnRadius,
                        0f);
                    ship.Rotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
                    ship.IsActive = true;

                    _context.Ships.RegisterShip(systemId, ship);
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

                    float radius = ship.Position.magnitude;
                    if (radius < 0.5f)
                        radius = SpawnRadius;

                    // Берём угол из текущей ориентации и продвигаем его по окружности
                    float angle = ship.Rotation.eulerAngles.z * Mathf.Deg2Rad;
                    float angularSpeed = BaseAngularSpeed + ship.Speed * 0.05f;
                    angle += angularSpeed * dt;

                    ship.Position = new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius,
                        0f);
                    ship.Rotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);

                    _context.Systems.TryUpdateShip(systemId, slot, in ship);
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
