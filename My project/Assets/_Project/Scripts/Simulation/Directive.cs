using _Project.Scripts.Core;
using UnityEngine;

namespace _Project.Scripts.Simulation
{
    public struct Directive
    {
        public EDerectiveType Type;    // Тип действия
        public Vector3 TargetPos;      // Целевая позиция (для MoveTo)
        public UID TargetUid;          // Цель по UID (для Attack, например)
        public float DesiredSpeed;     // Желаемая скорость (0-1)
        public int HorizonSteps;       // Сколько шагов актуально для этого указания
    }
}
