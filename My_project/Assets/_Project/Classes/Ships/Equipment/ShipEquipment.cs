using System; // для Serializable

namespace _Project.Scripts.Ships
{
    [Serializable]
    public struct ShipEquipment // оборудование корабля (контейнер подсистем)
    {
        public WeaponBlock Weapons; // подсистема оружия (хранит слоты и их состояние)

        public void Init(byte weaponSlotsCount) // инициализация подсистем (минимум: оружие)
        {
            Weapons.Init(weaponSlotsCount); // прокидываем инициализацию в блок оружия
        }
    }
}
