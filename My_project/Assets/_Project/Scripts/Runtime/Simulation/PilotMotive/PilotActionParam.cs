using UnityEngine;
using _Project.Scripts.Core;

namespace _Project.Scripts.Simulation.PilotMotivation
{
    /// <summary>
    /// Payload for a pilot action. Acts as a tagged union via the owning PilotAction.Action value.
    /// </summary>
    public struct PilotActionParam
    {
        public MoveParameters Move;
        public AttackParameters Attack;
        public AcquireParameters Acquire;

        public struct MoveParameters
        {
            public Vector3 Destination;
            public float DesiredSpeed;
            public float ArriveDistance;
        }

        public struct AttackParameters
        {
            public UID Target;
            public float DesiredRange;
            public bool AllowFriendlyFire;
        }

        public struct AcquireParameters
        {
            public float SearchRadius;
            public bool AllowFriendlyFire;
        }
    }
}
