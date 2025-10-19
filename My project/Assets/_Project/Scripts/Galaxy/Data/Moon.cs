using _Project.Scripts.Core;

namespace _Project.Scripts.Galaxy.Data
{
    public struct Moon
    {
        public Core.UID Uid;
        public string Name;            // Имя луны
        public EMoonType Type;          // Тип луны
        public EMoonSize Size;          // Размер
        public int OrbitIndex;         // Номер орбиты
        public float Mass;             // Масса
        public float Radius;           // Радиус
        public float OrbitDistance;    // Расстояние от планеты
        public float OrbitPeriod;      // Период обращения 
        public float Inclination;      // Наклон орбиты
        public float Atmosphere;       // Плотность атмосферы
        public float Temperature;      // Средняя температура поверхности
        public float Gravity;          // Ускорение свободного падения
    }
}