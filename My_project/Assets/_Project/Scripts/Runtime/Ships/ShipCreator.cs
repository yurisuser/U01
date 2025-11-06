using _Project.Scripts.Core;
using _Project.Scripts.NPC.Fraction;
using UnityEngine;

namespace _Project.Scripts.Ships
{
    public static class ShipCreator
    {
        public static Ship CreateShip(Fraction frac, UID pilotUid)
        {
            return new Ship(
                UIDService.Create(EntityType.Ship),
                pilotUid,
                frac,
                GetShipType(),               // тип корабля
                GetPosition(),           // позиция
                GetRotation(),           // ориентация
                GetHp(),                 // здоровье
                GetAgility(),
                GetIsActive()            // активность
            );
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

        private static float GetAgility()
        {
            return 0.3f;
        }

        private static bool GetIsActive()         // возвращает флаг активности
        {
            return true;
        }
    }
}
