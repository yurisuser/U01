using System; // для Serializable

namespace _Project.Scripts.Ships
{
    [Serializable]
    public struct WeaponEntity // завершённая сущность оружия с уже сгенерированными параметрами
    {
        public float Damage; // урон за выстрел
        public float Range; // дальность стрельбы

        public int Rate;    //скорострельность
    }
}
