using UnityEngine;

namespace _Project.Scripts.Simulation.Ships
{
    public struct ShipTask
    {
        public ShipTask(ShipTaskType type, ShipTaskParams parameters)
        {
            Type = type;
            Params = parameters;
        }

        public ShipTaskType Type;
        public ShipTaskParams Params;

        public static ShipTask MoveTo(Vector3 target, float tolerance, bool keepSpeed = false, _Project.Scripts.Core.UID targetUid = default)
        {
            return new ShipTask(ShipTaskType.MoveToPoint, new ShipTaskParams
            {
                TypeTask = ShipTaskType.MoveToPoint,
                MoveToPointParams = new MoveToPointParams
                {
                    Destination = target,
                    Tolerance = tolerance,
                    KeepSpeed = keepSpeed,
                    TargetUid = targetUid
                }
            });
        }

        public static ShipTask TradeBuy(_Project.Scripts.Core.UID stationUid, int itemId, int amount)
        {
            return new ShipTask(ShipTaskType.TradeBuy, new ShipTaskParams
            {
                TypeTask = ShipTaskType.TradeBuy,
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
            return new ShipTask(ShipTaskType.TradeSell, new ShipTaskParams
            {
                TypeTask = ShipTaskType.TradeSell,
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
