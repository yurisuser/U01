namespace _Project.Scripts.Stations
{
    /// <summary>Типы модулей станции.</summary>
    public enum EStationModuleType
    {
        None = 0,             // запасной вариант, модуль не задан
        Cargo = 1,            // склад/хранилище
        Dock = 2,             // стыковка кораблей
        ProductionLine = 3,   // производственная линия
        ResearchLab = 4,      // исследовательский модуль
        Hab = 5,              // жилой модуль
        Repair = 6,           // ремонтный модуль
        Trade = 7,            // торговый модуль
        Equipment = 8         // склады/верстаки оборудования
    }
}
