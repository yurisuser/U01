using UnityEngine;

namespace _Project.Scripts.Simulation.Ships
{
    public struct ShipTaskParams
    {
        public ShipTaskType TypeTask;
        public MoveToPointParams MoveToPointParams;
        public TradeBuyParams TradeBuyParams;
        public TradeSellParams TradeSellParams;
    }

    public struct MoveToPointParams
    {
        public Vector3 Destination;
        public float Tolerance;
        public bool KeepSpeed;
        public _Project.Scripts.Core.UID TargetUid;
    }

    public struct TradeBuyParams
    {
        public _Project.Scripts.Core.UID StationUid;
        public int ItemId;
        public int Amount;
    }

    public struct TradeSellParams
    {
        public _Project.Scripts.Core.UID StationUid;
        public int ItemId;
        public int Amount;
    }
}
