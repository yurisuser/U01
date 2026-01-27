namespace _Project.Scripts.Stations
{
    /// <summary>Конфиг торгового модуля.</summary>
    public sealed class TradeModuleData : IStationModuleData
    {
        public float PriceBuyMultiplier = 1.0f; // множитель цен покупки
        public float PriceSellMultiplier = 1.0f; // множитель цен продажи
        public EStationModuleType ModuleType => EStationModuleType.Trade; // идентификатор типа
    }
}
