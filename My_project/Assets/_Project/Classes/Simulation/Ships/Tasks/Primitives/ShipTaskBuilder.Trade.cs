namespace _Project.Scripts.Simulation.Ships
{
    public static partial class ShipTaskBuilder
    {
        public static ShipTask TradeBuy(_Project.Scripts.Core.UID stationUid, int itemId, int amount)
        {
            return new ShipTask(EShipTaskType.TradeBuy, new ShipTaskParams
            {
                TypeTask = EShipTaskType.TradeBuy,
                TradeBuyParams = new TradeBuyParams
                {
                    StationUid = stationUid,
                    ItemId = itemId,
                    Amount = amount
                }
            });
        }

        public static ShipTask TradeSell(_Project.Scripts.Core.UID stationUid, int itemId, int amount)
        {
            return new ShipTask(EShipTaskType.TradeSell, new ShipTaskParams
            {
                TypeTask = EShipTaskType.TradeSell,
                TradeSellParams = new TradeSellParams
                {
                    StationUid = stationUid,
                    ItemId = itemId,
                    Amount = amount
                }
            });
        }
    }
}
