using _Project.Scripts.Core;
using _Project.Scripts.Ships;

namespace _Project.Scripts.Simulation.Continuum
{
    /// <summary>Данные одного прыжка корабля через Continuum.</summary>
    public struct ContinuumTransit
    {
        public Ship Ship;                 // Оригинальный объект корабля на время прыжка
        public int FromSystemIndex;       // Индекс системы отправления
        public int ToSystemIndex;         // Индекс системы назначения
        public int RemainingTurns;        // Сколько ходов осталось лететь

        public static ContinuumTransit Create(
            Ship ship,
            int fromSystem,
            int toSystem,
            int remainingTurns)
        {
            return new ContinuumTransit
            {
                Ship = ship,
                FromSystemIndex = fromSystem,
                ToSystemIndex = toSystem,
                RemainingTurns = remainingTurns
            };
        }
    }
}
