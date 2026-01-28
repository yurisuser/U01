using UnityEngine;

namespace _Project.Scripts.Ships.Orders
{
    public struct TopShipOrderParams
    {
        public Vector3 Center;     // центр патруля
        public float Radius;       // радиус патруля
        public int SystemIndex;    // система для торговли/патруля
    }
}
