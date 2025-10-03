using System;

namespace _Project.Galaxy.Obj
{
    public struct Planet
    {
        public string name;         // Имя планеты
        public float mass;          // Масса (в условных ед.)
        public string type;         // Тип планеты (газовый гигант, каменная, ледяная и т.д.)
        public float atmosphere;    // Плотность атмосферы (0 = нет, 1 = земная, >1 = плотнее)
        public float radius;        // Радиус (в земных радиусах или условных ед.)
        public float orbitalDistance; // Расстояние от звезды (AU или условные ед.)
        public float orbitalPeriod; // Период обращения (в земных годах или условных ед.)
        public float temperature;   // Средняя температура поверхности (K)
        public float gravity;       // Гравитация на поверхности (g)
        public PlanetResource[] Resources;  // Какие ресурсы есть
    }
}