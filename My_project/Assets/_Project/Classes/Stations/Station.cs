using UnityEngine;
using _Project.Scripts.Core;
using _Project.Scripts.NPC.Fraction;

namespace _Project.Scripts.Stations
{
    /// <summary>Данные станции внутри StarSys.</summary>
    public struct Station
    {
        public UID Uid;                   // уникальный идентификатор станции
        public Fraction Owner;            // владелец/фракция
        public Vector3 Position;          // локально в системе
        public string TypeKey;            // ключ типа станции
        public string PrefabKey;          // ключ префаба в каталоге
        public StationModule[] Modules;   // набор модулей
        public float Hull;                // прочность корпуса
        public float PowerCapacity;       // ёмкость энергии
        public float PowerStored;         // текущий запас энергии
    }
}
