using UnityEngine;
using _Project.Scripts.Core;

namespace _Project.Scripts.Ships.Orders
{
    public struct TopShipOrderParams
    {
        public Vector3 Center;     // центр патруля
        public float Radius;       // радиус патруля
        public int SystemIndex;    // система для торговли/патруля
        public UID SellerStation;  // для межсистемной торговли
        public UID BuyerStation;   // для межсистемной торговли
        public int ItemId;         // товар
        public int Amount;         // количество
    }
}
