using UnityEngine;

namespace _Project.Scripts.Ships
{
    /// <summary>Сохраняемое состояние внутрисистемного варпа корабля.</summary>
    public struct ShipWarpState
    {
        public EShipWarpPhase Phase; // Текущая фаза движения; default означает Metric.
        public bool HasWarpDestination; // Есть ли активный приказ варпа к координате.
        public Vector3 WarpDestination; // Координата назначения варпа в пространстве системы.
        public Vector3 LockedDirection; // Курс, зафиксированный на время заряда, варпа и торможения.
        public int RemainingTurns; // Полные игровые ходы, оставшиеся в текущей фазе.
    }
}
