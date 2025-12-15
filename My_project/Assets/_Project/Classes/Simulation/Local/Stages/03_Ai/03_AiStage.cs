using UnityEngine;
using _Project.Scripts.Const;
using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.NPC.Fraction;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation;
using _Project.Scripts.Simulation.Ships;

namespace _Project.Scripts.Simulation.Local.Stages
{
    /// <summary>Тактическое принятие решений.</summary>
    public sealed class LocalAiStage : ILocalSimulationStage
    {
        public void Run(in LocalSimulationContext context)
        {
            var gameState = context.GameState;
            if (gameState == null || !context.HasActiveSystem)
                return;

            var galaxy = gameState.Galaxy;
            if (galaxy == null || galaxy.Length == 0)
                return;

            int systemIndex = gameState.SelectedSystemIndex;
            if (systemIndex < 0 || systemIndex >= galaxy.Length)
                return;

            var system = galaxy[systemIndex];
            var systemState = system.State;
            if (systemState == null)
            {
                // Гарантируем runtime-контекст для активной системы.
                systemState = new LocalSysRuntimeContext();
                system.State = systemState;
                galaxy[systemIndex] = system;
            }

            var ships = systemState.Ships;
            int deficit = SimulationConsts.ShipsPerSystem - ships.Count;
            if (deficit > 0) //если кораблей меньше минимума
            {
                // Дособираем тестовые корабли до целевого количества.
                for (int i = 0; i < deficit; i++)
                {
                    var ship = SpawnShip();
                    ship.Position = SamplePosition(SimulationConsts.SpawnRadius);
                    ship.Rotation = SampleOrientation();
                    ships.Add(ship);
                }
            }

            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                // Если у корабля нет задач, выдаём новую точку патруля.
                ShipTaskPlanner.EnsurePatrolTask(ref ship, SimulationConsts.SpawnRadius);
                ships[i] = ship;
            }
        }

        private static Ship SpawnShip()
        {
            var fractions = Fractions.All;
            Fraction fraction;
            if (fractions == null || fractions.Length == 0)
                fraction = new Fraction(EFraction.fraction1, "Default");
            else
            {
                int fracIndex = Random.Range(0, fractions.Length);
                fraction = fractions[fracIndex];
            }

            var pilotUid = UIDService.Create(EntityType.Individ);
            return ShipCreator.CreateShip(fraction, pilotUid);
        }

        private static Vector3 SamplePosition(float radius)
        {
            var offset = Random.insideUnitCircle * Mathf.Max(0f, radius);
            return new Vector3(offset.x, offset.y, 0f);
        }

        private static Quaternion SampleOrientation()
        {
            float yaw = Random.Range(0f, 360f);
            return Quaternion.Euler(0f, 0f, yaw);
        }
    }
}
