using System; // для Serializable

namespace _Project.Scripts.Ships
{
    [Serializable]
    public struct WeaponSlotState // состояние одного оружейного слота
    {
        public byte Installed; // 0 = пусто, 1 = установлено
        public WeaponEntity Weapon; // инлайн-данные оружия (без ссылок на каталоги)
        public float ShotsAccumulator; // накапливает дробную скорострельность

        public bool HasWeapon => Installed != 0; // true если оружие установлено
    }
}
