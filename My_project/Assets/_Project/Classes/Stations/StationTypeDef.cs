namespace _Project.Scripts.Stations
{
    /// <summary>Конфиг типа станции (ключ, модули, базовые статы).</summary>
    public sealed class StationTypeDef
    {
        public string Key;                    // идентификатор типа станции
        public EStationModuleType[] DefaultModules; // список модулей по умолчанию
        public string PrefabKey;              // ключ префаба в каталоге
        public float BaseHull;                // базовая прочность
        public float BasePower;               // базовая ёмкость/запас энергии
    }
}
