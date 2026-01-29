namespace _Project.Scripts.Trade.Models
{
    public enum ETradeFailReason
    {
        None = 0,
        InvalidInput,
        NotEnoughStock,
        NotEnoughCargoSpace,
        NotEnougMoney,
        OrderMissing,
    }
}
