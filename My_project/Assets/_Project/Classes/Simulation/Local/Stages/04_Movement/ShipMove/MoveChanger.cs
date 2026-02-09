using _Project.Scripts.Ships;

namespace _Project.Scripts.Simulation.Local.Stages.Movement
{
    /// <summary>Отвечает только за расчёт новой позиции по уже посчитанному шагу.</summary>
    public sealed class MoveChanger
    {
        public UnityEngine.Vector3 GetShift(in Ship ship, in UnityEngine.Vector3 stepShift)
        {
            if (stepShift.sqrMagnitude <= 0f)
                return ship.Position; // Сдвиг отсутствует — остаёмся в текущей точке.

            return ship.Position + stepShift; // Двигаем корабль на шаг, рассчитанный оркестратором движения.
        }
    }
}
