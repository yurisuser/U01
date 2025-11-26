using _Project.Scripts.Galaxy.Data;
using UnityEngine;

namespace _Project.Scripts.SystemMap
{
    /// <summary>Контракт слоя отрисовки системной карты.</summary>
    public interface ISystemMapLayer
    {
        int Order { get; } ///<summary>Порядок отрисовки слоя.</summary>
        void Init(Transform parentRoot); ///<summary>Инициализация слоя.</summary>
        void Render(in StarSys sys); ///<summary>Отрисовка слоя с данными системы.</summary>
        void Dispose(); ///<summary>Освобождение ресурсов слоя.</summary>
    }
}
