namespace _Project.Scripts.Trade.Models
{
    public struct TradeResult
    {
        public bool Success;
        public int MovedAmount;
        public ETradeFailReason FailReason;

        public static TradeResult Ok(int moved)
        {
            return new TradeResult
            {
                Success = true,
                MovedAmount = moved,
                FailReason = ETradeFailReason.None
            };
        }

        public static TradeResult Fail(ETradeFailReason reason)
        {
            return new TradeResult
            {
                Success = false,
                MovedAmount = 0,
                FailReason = reason
            };
        }
    }
}
