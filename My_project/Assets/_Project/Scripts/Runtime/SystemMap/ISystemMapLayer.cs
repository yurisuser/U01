using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using UnityEngine;
using _Project.Scripts.Simulation.Render;

namespace _Project.Scripts.SystemMap
{
    public interface ISystemMapLayer
    {
        int Order { get; }
        void Init(Transform parentRoot);
        void Render(in StarSys sys,
            Ship[] prevShips,
            int prevCount,
            Ship[] currShips,
            int currCount,
            Ship[] nextShips,
            int nextCount,
            float progress,
            float stepDuration,
            System.Collections.Generic.IReadOnlyDictionary<_Project.Scripts.Core.UID, System.Collections.Generic.List<_Project.Scripts.Simulation.Render.SubstepSample>> substeps);
        void Dispose();
    }
}
