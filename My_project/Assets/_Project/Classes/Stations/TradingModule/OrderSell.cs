namespace _Project.Scripts.Stations
{
    /// <summary>Ордер на продажу.</summary>
    public struct OrderSell
    {
        public _Project.Items.ItemKey Key; // тип + id предмета
        public int Price; // цена за единицу
        public int Amount; // сколько нужно продать
    }
}
