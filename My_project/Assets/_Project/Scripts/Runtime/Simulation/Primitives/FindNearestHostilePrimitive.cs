using _Project.Scripts.Core.Runtime;
using _Project.Scripts.NPC.Fraction;
using _Project.Scripts.Ships;

namespace _Project.Scripts.Simulation.Primitives
{
    internal static class FindNearestHostilePrimitive
    {
        public static bool TryFind(StarSystemState state, in Ship source, float radius, bool allowFriendlyFire, out TargetSnapshot snapshot, out int slot)
        {
            var buffer = state.ShipsBuffer;
            var count = state.ShipCount;
            float radiusSqr = radius > 0f ? radius * radius : float.PositiveInfinity;
            float bestDistance = float.PositiveInfinity;
            snapshot = default;
            slot = -1;

            for (int i = 0; i < count; i++)
            {
                var candidate = buffer[i];
                if (!candidate.IsActive || AreSameShip(candidate.Uid, source.Uid))
                    continue;

                if (!allowFriendlyFire && !FractionRelations.IsHostile(source.MakerFraction.Id, candidate.MakerFraction.Id))
                    continue;

                var delta = candidate.Position - source.Position;
                var distSqr = delta.sqrMagnitude;
                if (distSqr > radiusSqr || distSqr >= bestDistance)
                    continue;

                bestDistance = distSqr;
                snapshot = new TargetSnapshot(in candidate);
                slot = i;
            }

            return slot >= 0;
        }

        private static bool AreSameShip(in Core.UID a, in Core.UID b)
        {
            return a.Id == b.Id && a.Type == b.Type;
        }
    }
}
