using System.Collections.Generic;
using UnityEngine;
using _Project.Scripts.Core;
using _Project.Scripts.Ships;

namespace _Project.Scripts.Simulation.Render
{
    public struct SubstepSample
    {
        public float TimeFrac;   // 0..1 внутри тика
        public Vector3 Position;
        public Quaternion Rotation;
    }

    internal sealed class SubstepTraceBuffer
    {
        private readonly Dictionary<UID, List<SubstepSample>> _current = new();
        private readonly Dictionary<UID, List<SubstepSample>> _published = new();

        public IReadOnlyDictionary<UID, List<SubstepSample>> Published => _published;

        public void Clear()
        {
            _current.Clear();
            _published.Clear();
        }

        public void BeginTick()
        {
            _current.Clear();
        }

        public void AddSample(in UID uid, float timeFrac, in Vector3 pos, in Quaternion rot)
        {
            if (!_current.TryGetValue(uid, out var list))
            {
                list = new List<SubstepSample>(8);
                _current[uid] = list;
            }

            list.Add(new SubstepSample
            {
                TimeFrac = timeFrac,
                Position = pos,
                Rotation = rot
            });
        }

        public void Publish()
        {
            _published.Clear();
            foreach (var kvp in _current)
            {
                var source = kvp.Value;
                var copy = new List<SubstepSample>(source.Count);
                copy.AddRange(source);
                _published[kvp.Key] = copy;
            }
        }
    }
}
