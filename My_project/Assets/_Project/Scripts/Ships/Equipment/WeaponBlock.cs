using System; // для Serializable

namespace _Project.Scripts.Ships
{
    [Serializable]
    public struct WeaponBlock // блок оружия: слоты и операции с ними
    {
        public const int Capacity = 8; // жёсткий максимум слотов

        public byte Count; // активное число слотов (0..8)

        // 8 фиксированных слотов — без массивов, без GC
        public WeaponSlotState W0; // слот 0
        public WeaponSlotState W1; // слот 1
        public WeaponSlotState W2; // слот 2
        public WeaponSlotState W3; // слот 3
        public WeaponSlotState W4; // слот 4
        public WeaponSlotState W5; // слот 5
        public WeaponSlotState W6; // слот 6
        public WeaponSlotState W7; // слот 7

        public void Init(byte count) // задать количество слотов и сбросить состояние
        {
            if (count > Capacity) count = Capacity; // защита от переполнения
            Count = count; // записываем число слотов

            // сбрасываем состояние всех слотов
            W0 = default; // пусто
            W1 = default; // пусто
            W2 = default; // пусто
            W3 = default; // пусто
            W4 = default; // пусто
            W5 = default; // пусто
            W6 = default; // пусто
            W7 = default; // пусто
        }

        public bool IsValidIndex(int index) => (uint)index < Count; // проверка диапазона

        public WeaponSlotState GetSlot(int index) // получить копию слота
        {
            switch (index) // выбор по индексу без массивов
            {
                case 0: return W0; // слот 0
                case 1: return W1; // слот 1
                case 2: return W2; // слот 2
                case 3: return W3; // слот 3
                case 4: return W4; // слот 4
                case 5: return W5; // слот 5
                case 6: return W6; // слот 6
                case 7: return W7; // слот 7
                default: throw new ArgumentOutOfRangeException(nameof(index)); // некорректный индекс
            }
        }

        public void SetSlot(int index, in WeaponSlotState value) // записать слот по индексу
        {
            switch (index) // выбор по индексу без массивов
            {
                case 0: W0 = value; return; // слот 0
                case 1: W1 = value; return; // слот 1
                case 2: W2 = value; return; // слот 2
                case 3: W3 = value; return; // слот 3
                case 4: W4 = value; return; // слот 4
                case 5: W5 = value; return; // слот 5
                case 6: W6 = value; return; // слот 6
                case 7: W7 = value; return; // слот 7
                default: throw new ArgumentOutOfRangeException(nameof(index)); // некорректный индекс
            }
        }
    }
}
