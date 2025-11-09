using System.Text; // для построения строки
using UnityEngine;

namespace _Project.Scripts.Ships
{
    [DisallowMultipleComponent]
    public sealed class ShipClickReporter : MonoBehaviour // выводит статы/слоты при клике по кораблю
    {
        private ShipStats _stats; // последние статы корабля
        private WeaponBlock _weapons; // состояние оружейных слотов

        public void SetData(in Ship ship) // обновить данные из Ship
        {
            _stats = ship.Stats; // сохраняем статы
            _weapons = ship.Equipment.Weapons; // сохраняем блок оружия
        }

        private void OnMouseDown() => ReportClick(); // для старого Input System

        public void ReportClick() // вывести текущие данные
        {
            var msg = BuildReport(); // формируем строку отчёта
            Debug.Log(msg, this); // выводим в консоль
        }

        private string BuildReport() // собрать строку со статами и слотами
        {
            var sb = new StringBuilder(); // буфер
            sb.AppendLine($"Ship stats -> HP:{_stats.Hp}, MaxSpeed:{_stats.MaxSpeed:0.##}, Agility:{_stats.Agility:0.###}");
            sb.AppendLine($"Weapon slots: {_weapons.Count}");

            for (int i = 0; i < _weapons.Count; i++) // перечисляем слоты
            {
                var slot = _weapons.GetSlot(i); // копия слота
                var state = slot.HasWeapon ? $"Damage={slot.Weapon.Damage:0.##}, Range={slot.Weapon.Range:0.##}" : "empty"; // информация о слоте
                sb.AppendLine($"  Slot {i}: {state}");
            }

            return sb.ToString(); // готовый текст
        }
    }
}
