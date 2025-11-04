using UnityEngine;

namespace _Project.Scripts.Simulation.PilotMotivation
{
    public struct PilotAction
    {
        public EAction Action;
        public PilotActionParam Parameters;

        public static PilotAction CreateMoveTo(in Vector3 destination, float desiredSpeed, float arriveDistance)
        {
            return new PilotAction
            {
                Action = EAction.MoveToCoordinates,
                Parameters = new PilotActionParam
                {
                    Move = new PilotActionParam.MoveParameters
                    {
                        Destination = destination,
                        DesiredSpeed = desiredSpeed,
                        ArriveDistance = arriveDistance
                    }
                }
            };
        }
    }
}
