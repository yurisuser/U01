using UnityEngine;

namespace _Project.Scripts.Simulation
{
    /// <summary>
    /// Простая структура, описывающая активный мотив пилота.
    /// </summary>
    public struct PilotMotiv
    {
        public PilotOrderType Order;   // какой приказ сейчас выполняем
        public float DesiredSpeed;     // желаемая скорость (ед/с)
        public float WaitTimer;        // сколько ещё стоим на месте (сек)
        public PatrolState Patrol;     // состояние мотива патруля

        public static PilotMotiv Idle()
        {
            return new PilotMotiv
            {
                Order = PilotOrderType.Idle,
                DesiredSpeed = 0f,
                WaitTimer = 0f,
                Patrol = default
            };
        }

        public struct PatrolState
        {
            public Vector3 Center;        // центр области патрулирования
            public float Radius;          // максимальный радиус от центра
            public Vector3 CurrentTarget; // текущая цель
            public bool HasTarget;        // флаг, что цель уже выбрана
            public uint RandomState;      // внутреннее состояние генератора точек патруля
        }
    }
}
