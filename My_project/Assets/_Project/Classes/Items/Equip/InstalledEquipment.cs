using System; // для Serializable
using _Project.Items; // для ItemType

namespace _Project.Scripts.Ships
{
    [Serializable]
    public struct InstalledEquipment // контейнер установленного оборудования
    {
        public ItemType Type;   // тип предмета (Weapon/Engine/Shield/Scanner)
        public int Id;          // идентификатор из каталога
        public WeaponEntity Weapon;   // payload для оружия
        public EngineEntity Engine;   // payload для двигателя
        public ShieldEntity Shield;   // payload для щита
        public ScannerEntity Scanner; // payload для сканера

        public bool IsEmpty => Type == ItemType.None; // true, если слот пуст
    }
}
