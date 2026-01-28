using _Project.Scripts.Core;

namespace _Project.Scripts.Ships.Actions
{
    /// <summary>Действие корабля, выполняемое при достижении цели.</summary>
    public struct ShipAction
    {
        public EShipActionType Type;
        public UID TargetUid;
        public int ItemId;
        public int Amount;

        public bool IsEmpty => Type == EShipActionType.None;
    }
}
