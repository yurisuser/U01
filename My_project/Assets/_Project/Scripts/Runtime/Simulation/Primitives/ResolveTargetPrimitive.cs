using _Project.Scripts.Core;
using _Project.Scripts.Core.Runtime;

namespace _Project.Scripts.Simulation.Primitives
{
    internal static class ResolveTargetPrimitive
    {
        public static bool TryResolve(StarSystemState state, in UID targetUid, out TargetSnapshot snapshot, out int slot)
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

        private static bool AreSameShip(in UID a, in UID b)
        {
            return a.Id == b.Id && a.Type == b.Type;
        }

        private static bool IsValidUid(in UID uid)
        {
            return uid.Id != 0;
        }
    }
}
