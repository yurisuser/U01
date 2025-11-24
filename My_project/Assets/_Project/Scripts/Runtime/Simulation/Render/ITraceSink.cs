using _Project.Scripts.Core;
using UnityEngine;

namespace _Project.Scripts.Simulation.Render
{
    /// <summary>Приёмник точек трассировки перемещения.</summary>
    internal interface ITraceSink
    {
        void AddSample(in UID uid, float timeFrac, in Vector3 pos, in Quaternion rot);
    }
}
