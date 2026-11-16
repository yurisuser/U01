using _Project.Scripts.Simulation;
using _Project.Scripts.Simulation.Ships;
using _Project.Scripts.Simulation.AI;
using _Project.Scripts.Stations;
using _Project.Scripts.Simulation.Ships.Movement;

namespace _Project.Scripts.Simulation.Local.Stages.Ai
{
    /// <summary>Тактическое принятие решений.</summary>
    public sealed class LocalAiStage : ILocalSimulationStage
    {
        public void Run(in LocalSimulationContext context)
        {
            AdvanceNewAi(in context);
        }

        private static void AdvanceNewAi(in LocalSimulationContext context)
        {
            if (!context.HasActiveSystem || context.ActiveSystem.Value.State == null)
                return;

            var system = context.ActiveSystem.Value;
            StationTickService.TickTurn(in system, context.Day);
            IndustryTradeOrderSynchronizer.Refresh(in system);
            if (!context.IsTurnStart)
                return; // Решения и новые задачи применяются только на границе хода.
            var ships = system.State.Ships;
            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                ShipWarpTaskProcessor.BeginTurn(ref ship);
                ShipAiLegacyOrderBridge.TryMigratePatrol(ref ship);
                ShipAiController.Advance(ref ship, in system, context.DeltaTime);
                ships[i] = ship;
            }
        }
    }
}
