namespace _Project.Scripts.Galaxy.Data
{
    public struct Planet
    {
        public string Name;         // Имя планеты
        public float Mass;          // Масса (в условных ед.)
        public EPlanetType Type;         // Тип планеты (газовый гигант, каменная, ледяная и т.д.)
        public float Atmosphere;    // Плотность атмосферы (0 = нет, 1 = земная, >1 = плотнее)
        public float Radius;        // Радиус (в земных радиусах или условных ед.)
        public float OrbitalDistance; // Расстояние от звезды (AU или условные ед.)
        public float OrbitalPeriod; // Период обращения (в земных годах или условных ед.)
        public float Temperature;   // Средняя температура поверхности (K)
        public float Gravity;       // Гравитация на поверхности (g)
        public PlanetResource[] Resources;  // Какие ресурсы есть
    }
}