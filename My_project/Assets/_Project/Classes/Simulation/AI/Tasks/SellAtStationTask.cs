using UnityEngine;
using _Project.Items;
using _Project.Scripts.Core;

namespace _Project.Scripts.Simulation.AI
{
    public sealed class SellAtStationTask : StationTradeTask
    {
        public SellAtStationTask(UID stationUid, Vector3 stationPosition, ItemKey key, int amount, float tolerance)
            : base(EShipAiTaskType.SellAtStation, stationUid, stationPosition, key, amount, tolerance)
        {
        }
    }
}
