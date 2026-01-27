namespace _Project.Scripts.Stations
{
    /// <summary>Ордер на покупку.</summary>
    public struct OrderBy
    {
        public TypeTradeItem Type; // тип товара (goods/sku)
        public int ItemId; // id товара/ресурса
        public int Price; // цена за единицу
        public int Amount; // сколько нужно купить
    }
}
