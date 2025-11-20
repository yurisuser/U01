using UnityEngine;
using _Project.Scripts.Core;

namespace _Project.Scripts.Simulation.PilotMotivation
{
    /// <summary>Набор параметров для действия пилота; используется как объединение.</summary>
    public struct PilotActionParam
    {
        public MoveParameters Move; // Параметры перемещения.
        public AttackParameters Attack; // Параметры атаки.
        public CheckParameters Acquire; // Параметры поиска цели.

        public struct MoveParameters
        {
            public Vector3 Destination; // Цель маршрута.
            public float DesiredSpeed; // Желаемая скорость.
            public float ArriveDistance; // Радиус прибытия.
        }

        public struct AttackParameters
        {
            public UID Target; // Цель атаки.
            public float DesiredRange; // Дистанция боя.
            public bool AllowFriendlyFire; // Можно ли бить своих.
        }

        public struct CheckParameters
        {
            public float SearchRadius; // Радиус поиска.
            public bool AllowFriendlyFire; // Игнорируем ли союзность.
        }
    }
}
