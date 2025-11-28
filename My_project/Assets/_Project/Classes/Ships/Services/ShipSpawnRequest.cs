using _Project.Scripts.Core;
using _Project.Scripts.NPC.Fraction;
using UnityEngine;

namespace _Project.Scripts.Ships
{
    /// <summary>Заявка на спавн корабля в системе.</summary>
    public readonly struct ShipSpawnRequest
    {
        public ShipSpawnRequest(int systemIndex, Fraction fraction, UID pilotUid, Vector3 position, Quaternion rotation)
        {
            SystemIndex = systemIndex;
            Fraction = fraction;
            PilotUid = pilotUid;
            Position = position;
            Rotation = rotation;
        }

        public int SystemIndex { get; }       // индекс системы, куда спавнить
        public Fraction Fraction { get; }     // фракция корабля
        public UID PilotUid { get; }          // пилот (NPC/пустой UID)
        public Vector3 Position { get; }      // стартовая позиция
        public Quaternion Rotation { get; }   // стартовая ориентация
    }
}
