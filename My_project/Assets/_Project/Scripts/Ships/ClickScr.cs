using System.Text; // для StringBuilder
using _Project.Items; // для ItemType
using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState.GameStateMembers.SelectedObj;
using _Project.Scripts.Selection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Ships
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SelectableData))]
    public sealed class ClickScr : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        private SelectableData _data;

        private void Awake()
        {
            if (!targetCamera)
                targetCamera = Camera.main;
            _data = GetComponent<SelectableData>();
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null)
                return;

            bool left = mouse.leftButton.wasPressedThisFrame;
            bool right = mouse.rightButton.wasPressedThisFrame;
            if (!left && !right)
                return;

            if (!targetCamera)
                return;

            var ray = targetCamera.ScreenPointToRay(mouse.position.ReadValue());
            if (!Physics.Raycast(ray, out var hit))
                return;

            if (!hit.transform || !hit.transform.IsChildOf(transform))
                return;

            if (left)
                OnLeftClick();
            else if (right)
                OnRightClick();
        }

        private void OnLeftClick()
        {
            Debug.Log("Клик ЛКМ по кораблю", this);
            SetSelected();
        }

        private void OnRightClick()
        {
            LogEquipment();
        }

        private void SetSelected()
        {
            if (_data == null || !_data.HasData)
            {
                Debug.LogWarning("Клик по кораблю, но не заданы данные (UID/система).", this);
                return;
            }

            int systemIndex = _data.SystemIndex >= 0 ? _data.SystemIndex : GameBootstrap.GameState.SelectedSystemIndex;
            Debug.Log($"Данные выбора: SystemIndex={systemIndex}, UID={_data.Uid.Type}/{_data.Uid.Id}, Type={_data.SelectedType}", this);
            GameBootstrap.GameState.SelectedService.SetSelected(systemIndex, _data.Uid, _data.SelectedType);
        }

        private void LogEquipment()
        {
            if (_data == null || !_data.HasData)
            {
                Debug.LogWarning("ПКМ по кораблю, но не заданы данные (UID/система).", this);
                return;
            }

            int systemIndex = _data.SystemIndex >= 0 ? _data.SystemIndex : GameBootstrap.GameState.SelectedSystemIndex;
            var galaxy = GameBootstrap.GameState.Galaxy;
            if (galaxy == null || systemIndex < 0 || systemIndex >= galaxy.Length)
            {
                Debug.LogWarning("ПКМ по кораблю: неверный индекс системы.", this);
                return;
            }

            var system = galaxy[systemIndex];
            var runtime = system.State;
            if (runtime == null)
            {
                Debug.LogWarning("ПКМ по кораблю: в системе нет runtime-состояния.", this);
                return;
            }

            var uid = _data.Uid;
            var ships = runtime.Ships;
            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                if (ship.Uid.Type != uid.Type || ship.Uid.Id != uid.Id)
                    continue;

                Debug.Log(BuildEquipLog(in ship), this);
                return;
            }

            Debug.LogWarning("ПКМ по кораблю: корабль не найден в runtime-системе.", this);
        }

        private static string BuildEquipLog(in Ship ship)
        {
            var equip = ship.Equipment;
            var sb = new StringBuilder(256);
            sb.Append("Оборудование корабля UID=");
            sb.Append(ship.Uid.Type);
            sb.Append('/');
            sb.Append(ship.Uid.Id);
            sb.Append(" | оружие слотов=");
            sb.Append(equip.WeaponSlotsCount);

            for (int i = 0; i < equip.WeaponSlotsCount; i++)
            {
                sb.Append(" | W");
                sb.Append(i);
                sb.Append(": ");
                var slot = equip.GetWeaponSlot(i);
                sb.Append(FormatSlot(in slot));
            }

            sb.Append(" | Engine: ");
            sb.Append(FormatSlot(in equip.Engine));
            sb.Append(" | Shield: ");
            sb.Append(FormatSlot(in equip.Shield));
            sb.Append(" | Scanner: ");
            sb.Append(FormatSlot(in equip.Scanner));

            return sb.ToString();
        }

        private static string FormatSlot(in EquipSlot slot)
        {
            if (slot.IsEmpty)
                return "пусто";

            switch (slot.Type)
            {
                case ItemType.Weapon:
                    return $"Weapon id={slot.Id} dmg={slot.Weapon.Damage} rng={slot.Weapon.Range} rate={slot.Weapon.Rate}";
                case ItemType.Engine:
                    return $"Engine id={slot.Id} maxSpeed={slot.Engine.MaxSpeed} accel={slot.Engine.Acceleration} agility={slot.Engine.Agility}";
                case ItemType.Shield:
                    return $"Shield id={slot.Id} radius={slot.Shield.Radius} volume={slot.Shield.Volume} regen={slot.Shield.Regen}";
                case ItemType.Scanner:
                    return $"Scanner id={slot.Id} radius={slot.Scanner.Radius}";
                default:
                    return $"тип={slot.Type} id={slot.Id}";
            }
        }
    }
}
