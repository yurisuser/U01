namespace _Project.Scripts.Ships.Orders
{
    public struct TopShipOrder
    {
        public ETopShipOrderType Type;
        public TopShipOrderParams Params;

        public bool IsEmpty => Type == ETopShipOrderType.None;
    }
}
