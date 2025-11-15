using _Project.Scripts.Core;
using UnityEngine;
using _Project.Scripts.NPC.Fraction;
using _Project.Scripts.Ships;

namespace _Project.Scripts.Simulation.Primitives
{
    internal readonly struct TargetSnapshot
    {
        public readonly UID Uid;
        public readonly Vector3 Position;
        public readonly Vector3 Velocity;
        public readonly bool IsActive;
        public readonly EFraction Fraction;

        public TargetSnapshot(in Ship ship)
        {
            Uid = ship.Uid;
            Position = ship.Position;
            Velocity = ship.Velocity;
            IsActive = ship.IsActive;
            Fraction = ship.MakerFraction.Id;
        }
    }
}
