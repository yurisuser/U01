using UnityEngine;
using _Project.Items;
using _Project.Scripts.Core;

namespace _Project.Scripts.Simulation.AI
{
    /// <summary>Общие неизменяемые параметры покупки или продажи на станции.</summary>
    public abstract class StationTradeTask : ShipAiTask
    {
        protected StationTradeTask(EShipAiTaskType type, UID stationUid, Vector3 stationPosition, ItemKey key, int amount, float tolerance)
            : base(type)
        {
            StationUid = stationUid;
            StationPosition = stationPosition;
            Key = key;
            Amount = amount;
            Tolerance = tolerance;
        }

        public UID StationUid { get; }
        public Vector3 StationPosition { get; }
        public ItemKey Key { get; }
        public int Amount { get; }
        public float Tolerance { get; }
    }
}
