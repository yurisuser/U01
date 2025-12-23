using System; // для Serializable
using _Project.Items; // для ItemType

namespace _Project.Scripts.Ships
{
    [Serializable]
    public struct EquipSlot // установленное оборудование (один слот)
    {
        public ItemType Type;   // тип предмета
        public int Id;          // идентификатор из каталога
        public WeaponEntity Weapon;   // данные оружия
        public EngineEntity Engine;   // данные двигателя
        public ShieldEntity Shield;   // данные щита
        public ScannerEntity Scanner; // данные сканера

        public bool IsEmpty => Type == ItemType.None; // true, если слот пуст
    }
}
