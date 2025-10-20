using _Project.Scripts.Galaxy.Data;
using UnityEngine;

namespace _Project.Scripts.SystemMap
{
    public interface ISystemMapLayer
    {
        int Order { get; }                       // Приоритет вызова: меньше → раньше (сначала Geo, потом Ships)
        void Init(Transform parentRoot);         // Инициализация слоя; создаём свои корни/кэш/пулы под parentRoot
        void Render(in StarSys sys);             // Инкрементальный рендер: дифф по своим сущностям из sys
        void Dispose();                          // Очистка: вернуть объекты в пулы/освободить ссылки
    }
}