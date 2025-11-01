using UnityEngine;

namespace _Project.Scripts.Simulation
{
    /// <summary>
    /// Простая структура, описывающая активный мотив пилота.
    /// </summary>
    public struct PilotMotiv
    {
        public PilotOrderType Order;   // какой приказ сейчас выполняем
        public Vector3[] Waypoints;    // точки маршрута (для патруля)
        public int CurrentIndex;       // индекс текущей цели в массиве
        public float DesiredSpeed;     // желаемая скорость (ед/с)
        public float WaitTimer;        // сколько ещё стоим на месте (сек)

        public static PilotMotiv Idle()
        {
            return new PilotMotiv
            {
                Order = PilotOrderType.Idle,
                Waypoints = null,
                CurrentIndex = 0,
                DesiredSpeed = 0f,
                WaitTimer = 0f
            };
        }
    }
}
