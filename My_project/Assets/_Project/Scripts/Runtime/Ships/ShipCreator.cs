using _Project.DataAccess;
using _Project.Scripts.Core;
using _Project.Scripts.NPC.Fraction;
using UnityEngine;

namespace _Project.Scripts.Ships
{
    public static class ShipCreator
    {
        public static Ship CreateShip(Fraction frac, UID pilotUid)
        {
            var catalog = ShipCatalogReader.GetRandomShip();
            var shipType = ResolveShipType(in catalog);

            var ship = new Ship(
                UIDService.Create(EntityType.Ship),
                pilotUid,
                frac,
                shipType,
                GetPosition(),
                GetRotation(),
                catalog.Hp,
                catalog.MaxSpeed,
                catalog.Agility,
                GetIsActive()
            );

            ship.Equipment.Init(catalog.WeaponSlots);
            return ship;
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

        private static EShipType ResolveShipType(in CatalogShip ship)
        {
            if (ship.WeaponSlots <= 0 || ship.Key.Contains("transport"))
                return EShipType.Transport;
            return EShipType.Fighter;
        }
    }
}
