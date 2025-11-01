using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Core.Runtime;
using _Project.Scripts.NPC.Fraction;
using _Project.Scripts.Ships;
using UnityEngine;

namespace _Project.Scripts.Simulation
{
    /// <summary>
    /// Центральный исполнитель игрового шага: тик задач, обновление кораблей и синхронизация снапшотов.
    /// </summary>
    public sealed class Executor
    {
        private const int ShipsPerSystem = 3;          // сколько кораблей создаём на старт каждой системы
        private const float SpawnRadius = 6f;          // расстояние от центра системы до стартовой точки корабля

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
                _context.Ships.Tick(dt); // позже здесь появится реальное обновление приказов и движения
            }

            DoLogicStep(ref snapshot, dt);

            // Обновляем снапшот для UI, чтобы карта увидела новые позиции/добавления
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

                    // Разбрасываем корабли по окружности, чтобы они не слипались в одну точку
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

        private static void DoLogicStep(ref GameStateService.Snapshot snapshot, float dt)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"Logic tick: {snapshot.TickIndex}, dt={dt:0.###}");
#endif
        }
    }
}
