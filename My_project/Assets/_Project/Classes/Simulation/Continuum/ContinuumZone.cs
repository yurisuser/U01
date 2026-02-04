using UnityEngine;

namespace _Project.Scripts.Simulation.Continuum
{
    /// <summary>Переходная зона в системе для выхода в Continuum.</summary>
    public readonly struct ContinuumZone
    {
        public ContinuumZone(int systemIndex, int targetSystemIndex, Vector3 center, float radius, Vector3 direction)
        {
            SystemIndex = systemIndex;
            TargetSystemIndex = targetSystemIndex;
            Center = center;
            Radius = radius;
            Direction = direction;
        }

        public int SystemIndex { get; }        // Индекс системы, где расположена зона
        public int TargetSystemIndex { get; }  // Индекс системы назначения по этому гиперлинку
        public Vector3 Center { get; }         // Центр зоны в локальных координатах системы
        public float Radius { get; }           // Радиус зоны
        public Vector3 Direction { get; }      // Нормализованное направление из A в B (галк карта)
    }
}
