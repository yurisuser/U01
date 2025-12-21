using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Core.DebugState
{
    /// <summary>Хранилище отладочных путей.</summary>
    public sealed class DebugPathsState
    {
        private readonly Dictionary<int, DebugPath> _paths = new(64);

        public IEnumerable<KeyValuePair<int, DebugPath>> Paths => _paths;

        public void Clear()
        {
            _paths.Clear();
        }

        public void AddPoint(int uid, Vector3 position, Color color, int maxPoints)
        {
            if (!_paths.TryGetValue(uid, out var path))
            {
                path = new DebugPath(color);
                _paths.Add(uid, path);
            }

            path.Color = color;
            path.Points.Add(position);
            if (path.Points.Count > maxPoints && maxPoints > 0)
                path.Points.RemoveAt(0);
        }

        public void RemoveMissing(HashSet<int> keep)
        {
            if (keep.Count == 0)
            {
                _paths.Clear();
                return;
            }

            var toRemove = new List<int>();
            foreach (var id in _paths.Keys)
            {
                if (!keep.Contains(id))
                    toRemove.Add(id);
            }

            for (int i = 0; i < toRemove.Count; i++)
                _paths.Remove(toRemove[i]);
        }
    }

    public sealed class DebugPath
    {
        public DebugPath(Color color)
        {
            Color = color;
        }

        public Color Color { get; set; }
        public List<Vector3> Points { get; } = new List<Vector3>(64);
    }
}
