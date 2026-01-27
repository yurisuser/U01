namespace _Project.Scripts.Stations
{
    /// <summary>Ордер на продажу.</summary>
    public struct OrderSell
    {
        public TypeTradeItem Type; // тип товара (goods/sku)
        public int ItemId; // id товара/ресурса
        public int Price; // цена за единицу
        public int Amount; // сколько нужно продать
    }
}
