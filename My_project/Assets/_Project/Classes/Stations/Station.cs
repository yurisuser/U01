using UnityEngine;
using _Project.Scripts.Core;
using _Project.Items;
using _Project.Scripts.NPC.Fraction;
using _Project.Trade;

namespace _Project.Scripts.Stations
{
    /// <summary>Данные станции внутри StarSys.</summary>
    public struct Station
        : ITradeActor
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

        public Cargo Cargo => TryGetStorage()?.Cargo;

        Fraction ITradeActor.Owner => Owner;

        long ITradeActor.Money => Owner.Money;

        private StorageModuleState TryGetStorage()
        {
            if (Modules == null)
                return null;

            for (int i = 0; i < Modules.Length; i++)
            {
                var module = Modules[i];
                if (module == null || module.Type != EStationModuleType.Storage)
                    continue;

                return module.State as StorageModuleState;
            }

            return null;
        }
    }
}
