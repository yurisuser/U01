using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using UnityEngine;
using _Project.Scripts.Simulation.Render;

namespace _Project.Scripts.SystemMap
{
    /// <summary>Контракт слоя отрисовки системной карты.</summary>
    public interface ISystemMapLayer
    {
        int Order { get; } ///<summary>Порядок отрисовки слоя.</summary>
        void Init(Transform parentRoot); ///<summary>Инициализация слоя.</summary>
        void Render(in StarSys sys,
            Ship[] prevShips,
            int prevCount,
            Ship[] currShips,
            int currCount,
            Ship[] nextShips,
            int nextCount,
            float progress,
            float stepDuration,
            System.Collections.Generic.IReadOnlyDictionary<_Project.Scripts.Core.UID, System.Collections.Generic.List<_Project.Scripts.Simulation.Render.SubstepSample>> substeps); ///<summary>Отрисовка слоя с данными кораблей.</summary>
        void Dispose(); ///<summary>Освобождение ресурсов слоя.</summary>
    }
}
