namespace _Project.Scripts.Ships
{
    public struct ShipStats // базовые характеристики корабля
    {
        public int Hp;         // текущее здоровье
        public float MaxSpeed; // максимальная крейсерская скорость
        public float Agility;  // маневренность Рад/сек. Отвечает за поворот
        public float Acceleration; // ускорение/торможение
    }
}
