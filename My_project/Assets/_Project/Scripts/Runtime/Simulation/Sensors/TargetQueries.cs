using _Project.Scripts.Core.Runtime;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.PilotMotivation;
using _Project.Scripts.Simulation.Primitives;

namespace _Project.Scripts.Simulation.Sensors
{
    /// <summary>Квери для выбора/валидации целей ИИ.</summary>
    internal static class TargetQueries
    {
        /// <summary>Выбирает ближайшую подходящую цель для действия Acquire.</summary>
        public static bool TryAcquireNearestTarget(
            StarSystemState state,
            in Ship source,
            in PilotActionParam.CheckParameters request,
            out TargetSnapshot snapshot,
            out int slot)
        {
            if (state == null)
            {
                snapshot = default;
                slot = -1;
                return false;
            }

            return TargetingPrimitive.TryFindNearestHostile(
                state,
                in source,
                request.SearchRadius,
                request.AllowFriendlyFire,
                out snapshot,
                out slot);
        }
    }
}
