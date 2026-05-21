namespace _Project.Scripts.Stations
{
    /// <summary>Ордер на покупку.</summary>
    public struct OrderBy
    {
        public _Project.Items.ItemKey Key; // тип + id предмета
        public int Price; // цена за единицу
        public int Amount; // сколько нужно купить
    }
}
