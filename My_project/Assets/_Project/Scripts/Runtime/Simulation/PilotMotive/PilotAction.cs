using UnityEngine;
using _Project.Scripts.Core;

namespace _Project.Scripts.Simulation.PilotMotivation
{
    // Представляет действие пилота и параметры его выполнения.
    public struct PilotAction
    {
        public EAction Action; // Тип действия (движение, атака, поиск).
        public PilotActionParam Parameters; // Пакет параметров для выбранного поведения.

        // Создаёт действие перемещения к точке с указанными параметрами.
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

        // Создаёт действие атаки конкретной цели.
        public static PilotAction CreateAttackTarget(in UID target, float desiredRange, bool allowFriendlyFire)
        {
            return new PilotAction
            {
                Action = EAction.AttackTarget,
                Parameters = new PilotActionParam
                {
                    Attack = new PilotActionParam.AttackParameters
                    {
                        Target = target,
                        DesiredRange = Mathf.Max(0f, desiredRange),
                        AllowFriendlyFire = allowFriendlyFire
                    }
                }
            };
        }

        // Создаёт действие поиска ближайшей цели.
        public static PilotAction CreateAcquireTarget(float searchRadius, bool allowFriendlyFire)
        {
            return new PilotAction
            {
                Action = EAction.AcquireTarget,
                Parameters = new PilotActionParam
                {
                    Acquire = new PilotActionParam.CheckParameters
                    {
                        SearchRadius = Mathf.Max(0f, searchRadius),
                        AllowFriendlyFire = allowFriendlyFire
                    }
                }
            };
        }
    }
}
