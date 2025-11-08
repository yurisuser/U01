using _Project.Scripts.Core;
using _Project.Scripts.NPC.Fraction;
using UnityEngine;

namespace _Project.Scripts.Ships
{
    public static class ShipCreator
    {
        public static Ship CreateShip(Fraction frac, UID pilotUid)
        {
            var ship = new Ship( // создаём корабль (без оборудования)
                UIDService.Create(EntityType.Ship), // UID корабля
                pilotUid, // UID пилота
                frac, // фракция изготовителя
                GetShipType(), // тип корабля
                GetPosition(), // позиция
                GetRotation(), // ориентация
                GetHp(), // здоровье
                GetMaxSpeed(), // максимальная скорость
                GetAgility(), // маневренность
                GetIsActive() // активность
            );

            // Инициализируем и заполняем минимальное оборудование (только оружие)
            EquipmentGenerator.InitForShip(ref ship, fillWeapons: true); // генератор создаст слоты и пушки

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

        private static int GetHp()                // возвращает здоровье
        {
            return 100;
        }

        private static float GetMaxSpeed()        // возвращает максимальную скорость
        {
            return 30f;
        }

        private static float GetAgility()         // возвращает маневренность 0..1
        {
            return 0.1f;
        }

        private static bool GetIsActive()         // возвращает флаг активности
        {
            return true;
        }
    }
}
