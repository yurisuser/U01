using UnityEngine;
using _Project.Items;
using _Project.Scripts.Core;

namespace _Project.Scripts.Simulation.AI
{
    public sealed class BuyAtStationTask : StationTradeTask
    {
        public BuyAtStationTask(UID stationUid, Vector3 stationPosition, ItemKey key, int amount, float tolerance)
            : base(EShipAiTaskType.BuyAtStation, stationUid, stationPosition, key, amount, tolerance)
        {
        }
    }
}
