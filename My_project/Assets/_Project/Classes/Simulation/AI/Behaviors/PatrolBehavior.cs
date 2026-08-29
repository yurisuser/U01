using UnityEngine;

namespace _Project.Scripts.Simulation.AI
{
    /// <summary>Бесконечное поведение патруля: после каждой точки выбирает следующую.</summary>
    public sealed class PatrolBehavior : ShipAiBehavior
    {
        private readonly Vector3 _center;
        private readonly float _radius;
        private readonly float _tolerance;
        private readonly int _seed;
        private int _step;

        public PatrolBehavior(Vector3 center, float radius, float tolerance, int seed)
        {
            _center = center;
            _radius = Mathf.Max(0f, radius);
            _tolerance = Mathf.Max(0f, tolerance);
            _seed = seed;
        }

        public override bool TryCreateTask(in _Project.Scripts.Ships.Ship ship, in _Project.Scripts.Galaxy.Data.StarSys system, out ShipAiTask task)
        {
            task = new MoveToPointTask(GetNextPoint(), _tolerance, keepSpeed: true);
            return true;
        }

        public override bool IsCompletedBy(in ShipAiTaskResult result)
        {
            return false;
        }

        private Vector3 GetNextPoint()
        {
            _step++;
            float angle = Mathf.Repeat(_seed * 0.61803398875f + _step * 2.39996323f, Mathf.PI * 2f);
            float distance = _radius * (0.35f + 0.65f * Mathf.Repeat(_seed * 0.754877666f + _step * 0.414213562f, 1f));
            return _center + new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0f);
        }
    }
}
