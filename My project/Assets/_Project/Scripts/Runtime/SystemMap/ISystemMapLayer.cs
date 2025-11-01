using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using UnityEngine;

namespace _Project.Scripts.SystemMap
{
    public interface ISystemMapLayer
    {
        int Order { get; }
        void Init(Transform parentRoot);
        void Render(in StarSys sys, Ship[] prevShips, int prevCount, Ship[] currShips, int currCount, float progress);
        void Dispose();
    }
}
