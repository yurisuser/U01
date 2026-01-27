using System.Collections.Generic;

namespace _Project.Scripts.Stations
{
    /// <summary>Состояние торгового модуля.</summary>
    public sealed class TradeModuleState : IStationModuleState
    {
        public readonly Dictionary<int, OrderBy> OrdersBuy = new(); // ключ товара → ордер на покупку
        public readonly Dictionary<int, OrderSell> OrdersSell = new(); // ключ товара → ордер на продажу
    }
}
