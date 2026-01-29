namespace _Project.Scripts.Stations
{
    /// <summary>Ордер на покупку.</summary>
    public struct OrderBy
    {
        public _Project.Items.ItemType Type; // тип предмета
        public int ItemId; // id товара/ресурса
        public int Price; // цена за единицу
        public int Amount; // сколько нужно купить
    }
}
