using _Project.Scripts.Core;
using _Project.Scripts.NPC.Fraction;
using UnityEngine;

namespace _Project.Scripts.Ships
{
    public static class ShipCreator
    {
        public static Ship CreateShip(Fraction frac, UID pilotUid)
        {
            var shipType = GetShipType(); // определяем тип корабля
            var templateId = GetTemplateId(shipType); // выбираем шаблон для типа
            var config = ShipConfigGenerator.Create(templateId); // получаем конфигурацию корабля
            var stats = config.Stats; // базовые параметры

            var ship = new Ship( // создаём корабль с параметрами из конфигурации
                UIDService.Create(EntityType.Ship), // UID корабля
                pilotUid, // UID пилота
                frac, // фракция изготовителя
                shipType, // тип корабля
                GetPosition(), // позиция
                GetRotation(), // ориентация
                stats.Hp, // здоровье из конфигурации
                stats.MaxSpeed, // максимальная скорость
                stats.Agility, // маневренность
                GetIsActive() // активность
            );

            ship.Equipment.Init(config.WeaponSlotsCount); // создаём пустые слоты оружия

            return ship; // возвращаем готовый корабль
        }
        
        private static EShipType GetShipType()        // возвращает тип корабля
        {
            return EShipType.Fighter;
        }

        private static Vector3 GetPosition()      // возвращает мировую позицию
        {
            return Vector3.zero;
        }

        private static Quaternion GetRotation()   // возвращает ориентацию корабля
        {
            return Quaternion.identity;
        }

        private static bool GetIsActive()         // возвращает флаг активности
        {
            return true;
        }

        private static ShipConfigGenerator.TemplateId GetTemplateId(EShipType type) // выбор шаблона под тип
        {
            switch (type)
            {
                case EShipType.Transport:
                    return ShipConfigGenerator.TemplateId.Transport; // транспорт
                default:
                    return ShipConfigGenerator.TemplateId.Default; // базовый для остальных
            }
        }
    }
}
