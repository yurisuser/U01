using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using UnityEngine;

namespace _Project.Scripts.SystemMap
{
    public interface ISystemMapLayer
    {
        int Order { get; }                       // Порядок слоя: чем меньше значение, тем раньше рисуем
        void Init(Transform parentRoot);         // Готовим рабочие объекты, подвешенные к parentRoot
        void Render(in StarSys sys, Ship[] ships, int shipCount); // Обновляем слой с актуальной системой и её кораблями
        void Dispose();                          // Удаляем созданные объекты и чистим ссылки
    }
}
