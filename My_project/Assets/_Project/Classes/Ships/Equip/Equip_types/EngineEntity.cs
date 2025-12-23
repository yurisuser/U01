using System; // для Serializable

namespace _Project.Scripts.Ships
{
    [Serializable]
    public struct EngineEntity // данные установленного двигателя
    {
        public float MaxSpeed;     // максимальная скорость
        public float Acceleration; // ускорение/торможение
        public float Agility;      // вклад в манёвренность
    }
}
