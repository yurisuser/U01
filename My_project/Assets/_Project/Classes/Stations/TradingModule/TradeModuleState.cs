using System.Collections.Generic;
using _Project.Items;

namespace _Project.Scripts.Stations
{
    /// <summary>Состояние торгового модуля.</summary>
    public sealed class TradeModuleState : IStationModuleState
    {
        public readonly Dictionary<ItemKey, OrderBy> OrdersBuy = new(); // ключ товара → ордер на покупку
        public readonly Dictionary<ItemKey, OrderSell> OrdersSell = new(); // ключ товара → ордер на продажу
    }
}
