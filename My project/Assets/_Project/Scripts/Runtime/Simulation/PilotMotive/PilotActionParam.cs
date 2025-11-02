using UnityEngine;

namespace _Project.Scripts.Simulation.PilotMotivation
{
    /// <summary>
    /// Payload for a pilot action. Acts as a tagged union via the owning PilotAction.Action value.
    /// </summary>
    public struct PilotActionParam
    {
        public MoveParameters Move;

        public struct MoveParameters
        {
            public Vector3 Destination;
            public float DesiredSpeed;
            public float ArriveDistance;
        }
    }
}
