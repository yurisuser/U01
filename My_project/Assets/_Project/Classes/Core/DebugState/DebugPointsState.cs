using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Core.DebugState
{
    /// <summary>Хранилище отладочных точек.</summary>
    public sealed class DebugPointsState
    {
        private readonly List<DebugPoint> _points = new(64);

        public IReadOnlyList<DebugPoint> Points => _points;

        public void Clear()
        {
            _points.Clear();
        }

        public void AddPoint(int uid, Vector3 position, Color color)
        {
            _points.Add(new DebugPoint(uid, position, color));
        }
    }

    public readonly struct DebugPoint
    {
        public DebugPoint(int uid, Vector3 position, Color color)
        {
            Uid = uid;
            Position = position;
            Color = color;
        }

        public int Uid { get; }
        public Vector3 Position { get; }
        public Color Color { get; }
    }
}
