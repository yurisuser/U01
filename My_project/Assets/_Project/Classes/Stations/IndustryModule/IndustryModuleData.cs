using _Project.Industry.Recipes;

namespace _Project.Scripts.Stations
{
    /// <summary>Постоянная конфигурация индустриального модуля станции.</summary>
    public sealed class IndustryModuleData : IStationModuleData
    {
        public Recipe Recipe; // рецепт, установленный в модуль
        public EStationModuleType ModuleType => EStationModuleType.Industry;
    }
}
