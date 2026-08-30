using _Project.Scripts.Core;
using _Project.Scripts.NPC.Fraction;

namespace _Project.Scripts.Stations
{
    /// <summary>Создаёт станции из дефов.</summary>
    public static class StationFactory
    {
        public static Station Create(StationTypeDef def, UID uid, Fraction owner)
        {
            var station = new Station
            {
                Uid = uid,                              // идентификатор
                Owner = owner,                          // владелец
                TypeKey = def?.Key,                     // тип станции
                PrefabKey = def?.PrefabKey,             // ключ префаба
                Hull = def?.BaseHull ?? 0f,             // стартовая прочность
                PowerCapacity = def?.BasePower ?? 0f,   // ёмкость энергии
                PowerStored = def?.BasePower ?? 0f,     // стартовый запас
                Modules = BuildDefaultModules(def),     // модули по дефолту
            };

            return station;
        }

        private static StationModule[] BuildDefaultModules(StationTypeDef def)
        {
            if (def == null || def.DefaultModules == null || def.DefaultModules.Length == 0)
                return System.Array.Empty<StationModule>();

            var result = new StationModule[def.DefaultModules.Length];
            for (int i = 0; i < def.DefaultModules.Length; i++)
            {
                var type = def.DefaultModules[i];
                result[i] = new StationModule
                {
                    Type = type,
                    Level = 1,
                    Data = CreateModuleData(type),
                    State = CreateModuleState(type),
                };
            }

            return result;
        }

        private static IStationModuleData CreateModuleData(EStationModuleType type)
        {
            return type switch
            {
                EStationModuleType.Storage => new StorageModuleData(),
                EStationModuleType.Dock => new DockModuleData(),
                EStationModuleType.Industry => new IndustryModuleData(),
                EStationModuleType.Trade => new TradeModuleData(),
                _ => null
            };
        }

        private static IStationModuleState CreateModuleState(EStationModuleType type)
        {
            return type switch
            {
                EStationModuleType.Storage => new StorageModuleState(),
                EStationModuleType.Dock => new DockModuleState(),
                EStationModuleType.Industry => new IndustryModuleState(),
                EStationModuleType.Trade => new TradeModuleState(),
                _ => null
            };
        }
    }
}
