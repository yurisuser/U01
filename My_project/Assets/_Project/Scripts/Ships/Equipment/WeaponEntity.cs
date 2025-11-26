using System; // для Serializable

namespace _Project.Scripts.Ships
{
    [Serializable]
    public struct WeaponEntity // завершённая сущность оружия с уже сгенерированными параметрами
    {
        public BulletSpec Bullet; // визуальные и типовые параметры пули
        public float Damage; // урон за выстрел
        public float Range; // дальность стрельбы
        public int Rate;    //скорострельность
    }
}
