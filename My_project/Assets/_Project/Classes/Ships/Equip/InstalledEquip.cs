using System; // для Serializable

namespace _Project.Scripts.Ships
{
    [Serializable]
    public struct InstalledEquip // оборудование корабля по слотам
    {
        public const int MaxWeaponSlots = 8; // жёсткий предел слотов

        public byte WeaponSlotsCount; // активное число оружейных слотов (0..8)

        public EquipSlot W0;
        public EquipSlot W1;
        public EquipSlot W2;
        public EquipSlot W3;
        public EquipSlot W4;
        public EquipSlot W5;
        public EquipSlot W6;
        public EquipSlot W7;

        public EquipSlot Engine;
        public EquipSlot Shield;
        public EquipSlot Scanner;

        public void Init(byte weaponSlotsCount) // инициализация слотов
        {
            WeaponSlotsCount = weaponSlotsCount > MaxWeaponSlots ? (byte)MaxWeaponSlots : weaponSlotsCount;

            W0 = default;
            W1 = default;
            W2 = default;
            W3 = default;
            W4 = default;
            W5 = default;
            W6 = default;
            W7 = default;
            Engine = default;
            Shield = default;
            Scanner = default;
        }

        public bool IsValidWeaponIndex(int index) => (uint)index < WeaponSlotsCount; // проверка диапазона

        public EquipSlot GetWeaponSlot(int index) // получить слот оружия
        {
            switch (index)
            {
                case 0: return W0;
                case 1: return W1;
                case 2: return W2;
                case 3: return W3;
                case 4: return W4;
                case 5: return W5;
                case 6: return W6;
                case 7: return W7;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public void SetWeaponSlot(int index, in EquipSlot value) // записать слот оружия
        {
            switch (index)
            {
                case 0: W0 = value; return;
                case 1: W1 = value; return;
                case 2: W2 = value; return;
                case 3: W3 = value; return;
                case 4: W4 = value; return;
                case 5: W5 = value; return;
                case 6: W6 = value; return;
                case 7: W7 = value; return;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }
}
