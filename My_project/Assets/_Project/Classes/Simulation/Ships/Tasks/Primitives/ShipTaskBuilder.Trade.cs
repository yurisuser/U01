namespace _Project.Scripts.Simulation.Ships
{
    public static partial class ShipTaskBuilder
    {
        public static ShipTask TradeBuy(_Project.Scripts.Core.UID stationUid, _Project.Items.ItemKey key, int amount)
        {
            return new ShipTask(EShipTaskType.TradeBuy, new ShipTaskParams
            {
                TypeTask = EShipTaskType.TradeBuy,
                TradeBuyParams = new TradeBuyParams
                {
                    StationUid = stationUid,
                    Key = key,
                    Amount = amount
                }
            });
        }

        public static ShipTask TradeSell(_Project.Scripts.Core.UID stationUid, _Project.Items.ItemKey key, int amount)
        {
            return new ShipTask(EShipTaskType.TradeSell, new ShipTaskParams
            {
                TypeTask = EShipTaskType.TradeSell,
                TradeSellParams = new TradeSellParams
                {
                    StationUid = stationUid,
                    Key = key,
                    Amount = amount
                }
            });
        }
    }
}
