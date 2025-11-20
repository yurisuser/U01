using _Project.Scripts.Core;
using _Project.Scripts.Core.Runtime;
using _Project.Scripts.NPC.Fraction;
using _Project.Scripts.Ships;
using UnityEngine;

namespace _Project.Scripts.Simulation.Execution.Spawn
{
    // Отвечает за первичный спавн кораблей в системах.
    public sealed class ShipSpawnService
    {
        private readonly RuntimeContext _context; // Общий контекст симуляции.
        private readonly Motivator _motivator; // Мотиватор для назначения приказов пилотам.
        private bool _initialShipsSpawned; // Флаг, чтобы не спавнить корабли повторно.

        // Запоминаем зависимости генератора.
        public ShipSpawnService(RuntimeContext context, Motivator motivator)
        {
            _context = context;
            _motivator = motivator;
        }

        // Создаём стартовые корабли во всех системах.
        public void EnsureInitialShips()
        {
            if (_initialShipsSpawned || _context == null)
                return;

            var galaxyCount = _context.Galaxy?.Count ?? 0;
            if (galaxyCount <= 0)
                return;

            for (int systemId = 0; systemId < galaxyCount; systemId++)
            {
                for (int i = 0; i < SimulationConsts.ShipsPerSystem; i++)
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

                    float angle = i / (float)SimulationConsts.ShipsPerSystem * Mathf.PI * 2f;
                    float edgeRadius = SimulationConsts.SpawnRadius * 20f;
                    ship.Position = new Vector3(
                        Mathf.Cos(angle) * edgeRadius,
                        Mathf.Sin(angle) * edgeRadius,
                        0f);
                    ship.Rotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
                    ship.IsActive = true;

                    _context.Ships.RegisterShip(systemId, ship);

                    if (_context.Pilots != null)
                    {
                        float searchRadius = Mathf.Max(SimulationConsts.SpawnRadius * 10f, 250f);
                        var motiv = _motivator.CreateAttackAll(searchRadius, allowFriendlyFire: false);
                        _context.Pilots.SetMotiv(pilotUid, in motiv);
                    }
                }
            }

            _initialShipsSpawned = true;
        }

        // Определяем фракцию корабля по детерминированному псевдослучайному правилу.
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
    }
}
