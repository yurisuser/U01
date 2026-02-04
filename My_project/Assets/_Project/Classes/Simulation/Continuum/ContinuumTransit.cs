using _Project.Scripts.Core;
using UnityEngine;

namespace _Project.Scripts.Simulation.Continuum
{
    /// <summary>Данные одного прыжка корабля через Continuum.</summary>
    public struct ContinuumTransit
    {
        public UID ShipUid;                // UID корабля в полёте
        public int FromSystemIndex;        // Индекс системы отправления
        public int ToSystemIndex;          // Индекс системы назначения
        public int RemainingTurns;         // Сколько ходов осталось лететь
        public int StartDay;               // День старта прыжка
        public int ArrivalOrbitIndex;      // Орбита появления (1..3 по ТЗ)
        public Vector3 EntryDirection;     // Нормализованное направление A->B на галкарте

        public static ContinuumTransit Create(
            UID shipUid,
            int fromSystem,
            int toSystem,
            int startDay,
            int remainingTurns,
            int arrivalOrbitIndex,
            Vector3 entryDirection)
        {
            return new ContinuumTransit
            {
                ShipUid = shipUid,
                FromSystemIndex = fromSystem,
                ToSystemIndex = toSystem,
                StartDay = startDay,
                RemainingTurns = remainingTurns,
                ArrivalOrbitIndex = arrivalOrbitIndex,
                EntryDirection = entryDirection
            };
        }
    }
}
