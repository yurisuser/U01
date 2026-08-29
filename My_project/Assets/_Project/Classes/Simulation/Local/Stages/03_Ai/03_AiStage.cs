using _Project.Scripts.Simulation;
using _Project.Scripts.Simulation.Ships;
using _Project.Scripts.Simulation.AI;

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

            var ships = context.ActiveSystem.Value.State.Ships;
            var system = context.ActiveSystem.Value;
            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                ShipAiLegacyOrderBridge.TryMigratePatrol(ref ship);
                ShipAiController.Advance(ref ship, in system);
                ships[i] = ship;
            }
        }
    }
}
