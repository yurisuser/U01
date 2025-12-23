using System; // для Serializable

namespace _Project.Scripts.Ships
{
    [Serializable]
    public struct ShieldEntity // данные установленного щита
    {
        public float Radius; // радиус купола
        public float Volume; // запас прочности
        public float Regen;  // скорость регенерации
    }
}
