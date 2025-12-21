using _Project.Scripts.Core.DebugState;
using UnityEngine;

namespace _Project.Scripts.SystemMap.Debug
{
    /// <summary>Рисует отладочные точки через Gizmos.</summary>
    [DisallowMultipleComponent]
    public sealed class DebugPointsRenderer : MonoBehaviour
    {
        [SerializeField] private float pointRadius = 2f;
        private DebugPointsState _state;
        private Transform _root;

        public void Bind(DebugPointsState state)
        {
            _state = state;
        }

        public void SetRoot(Transform root)
        {
            _root = root;
        }

        private void OnDrawGizmos()
        {
            if (_state == null)
                return;

            var points = _state.Points;
            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];
                Gizmos.color = point.Color;
                var pos = _root ? _root.TransformPoint(point.Position) : point.Position;
                Gizmos.DrawSphere(pos, pointRadius);
            }
        }
    }
}
