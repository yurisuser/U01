namespace _Project.Scripts.Ships
{
    public struct ShipStats // базовые характеристики корабля
    {
        public int Hp;         // текущее здоровье
        public float WarpSpeed; // базовая варповая скорость до применения коэффициента
        public float MetricSpeed; // базовая метрическая скорость до применения коэффициента
        public float Agility;  // маневренность Рад/сек. Отвечает за поворот
        public float Acceleration; // ускорение/торможение
    }
}
