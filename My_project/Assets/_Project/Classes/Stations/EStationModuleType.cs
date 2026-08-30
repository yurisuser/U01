namespace _Project.Scripts.Stations
{
    /// <summary>Типы модулей станции.</summary>
    public enum EStationModuleType
    {
        None = 0,             // запасной вариант, модуль не задан
        Storage = 1,          // склад/хранилище
        Dock = 2,             // стыковка кораблей
        Industry = 3,          // добыча, переработка и производство
        ResearchLab = 4,      // исследовательский модуль
        Hab = 5,              // жилой модуль
        Repair = 6,           // ремонтный модуль
        Trade = 7,            // торговый модуль
        Equipment = 8         // склады/верстаки оборудования
    }
}
