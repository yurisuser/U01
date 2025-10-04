namespace _Project.Scripts.Galaxy.Data
{
    public struct Moon
    {
        public string Name;            // Имя луны
        public MoonType Type;          // Тип луны (каменистая, ледяная, вулканическая и т.д.)
        public MoonSize Size;          // Размер (Tiny, Small, Medium, Large)
        public int OrbitIndex;         // Номер орбиты
        public float Mass;             // Масса
        public float Radius;           // Радиус
        public float OrbitDistance;    // Расстояние от планеты
        public float OrbitPeriod;      // Период обращения (в днях/годах)
        public float Inclination;      // Наклон орбиты (в градусах)
        public float Atmosphere;       // Плотность атмосферы (0 = нет, 1 = земная, >1 = плотнее)
        public float Temperature;      // Средняя температура поверхности
        public float Gravity;          // Ускорение свободного падения
    }
}