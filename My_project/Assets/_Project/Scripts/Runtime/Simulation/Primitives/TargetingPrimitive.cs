using _Project.Scripts.Core;
using _Project.Scripts.Core.Runtime;
using _Project.Scripts.NPC.Fraction;
using _Project.Scripts.Ships;
using UnityEngine;

namespace _Project.Scripts.Simulation.Primitives
{
    internal static class TargetingPrimitive
    {
        public static bool TryResolveTarget(StarSystemState state, in UID targetUid, out TargetSnapshot snapshot, out int slot)
        {
            if (!IsValidUid(targetUid))
            {
                snapshot = default;
                slot = -1;
                return false;
            }

            var buffer = state.ShipsBuffer;
            var count = state.ShipCount;
            for (int i = 0; i < count; i++)
            {
                var candidate = buffer[i];
                if (!candidate.IsActive || !AreSameShip(candidate.Uid, targetUid))
                    continue;

                snapshot = new TargetSnapshot(in candidate);
                slot = i;
                return true;
            }

            snapshot = default;
            slot = -1;
            return false;
        }

        public static bool TryFindNearestHostile(StarSystemState state, in Ship source, float radius, bool allowFriendlyFire, out TargetSnapshot snapshot, out int slot)
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

        private static bool AreSameShip(in UID a, in UID b)
        {
            return a.Id == b.Id && a.Type == b.Type;
        }

        private static bool IsValidUid(in UID uid)
        {
            return uid.Id != 0;
        }
    }

    internal readonly struct TargetSnapshot
    {
        public readonly UID Uid;
        public readonly Vector3 Position;
        public readonly Vector3 Velocity;
        public readonly bool IsActive;
        public readonly EFraction Fraction;

        public TargetSnapshot(in Ship ship)
        {
            Uid = ship.Uid;
            Position = ship.Position;
            Velocity = ship.Velocity;
            IsActive = ship.IsActive;
            Fraction = ship.MakerFraction.Id;
        }
    }
}
