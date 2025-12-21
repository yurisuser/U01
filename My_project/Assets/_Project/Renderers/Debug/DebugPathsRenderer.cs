using _Project.Scripts.Core.DebugState;
using UnityEngine;

namespace _Project.Scripts.SystemMap.Debug
{
    /// <summary>Рисует отладочные пути через Gizmos.</summary>
    [DisallowMultipleComponent]
    public sealed class DebugPathsRenderer : MonoBehaviour
    {
        private DebugPathsState _state;
        private Transform _root;

        public void Bind(DebugPathsState state)
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

            foreach (var pair in _state.Paths)
            {
                var path = pair.Value;
                var points = path.Points;
                if (points.Count < 2)
                    continue;

                Gizmos.color = path.Color;
                for (int i = 1; i < points.Count; i++)
                {
                    var a = _root ? _root.TransformPoint(points[i - 1]) : points[i - 1];
                    var b = _root ? _root.TransformPoint(points[i]) : points[i];
                    Gizmos.DrawLine(a, b);
                }
            }
        }
    }
}
