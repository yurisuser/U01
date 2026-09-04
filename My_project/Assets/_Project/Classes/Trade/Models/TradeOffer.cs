using _Project.Items;

namespace _Project.Trade
{
    /// <summary>Данные сделки между двумя сторонами.</summary>
    public readonly struct TradeOffer
    {
        public readonly ITradeActor Seller;
        public readonly ITradeActor Buyer;
        public readonly ItemKey Key;
        public readonly int Amount;
        public readonly int UnitPrice;
        public readonly int SystemIndex;

        public TradeOffer(
            ITradeActor seller,
            ITradeActor buyer,
            ItemKey key,
            int amount,
            int unitPrice,
            int systemIndex)
        {
            Seller = seller;
            Buyer = buyer;
            Key = key;
            Amount = amount;
            UnitPrice = unitPrice;
            SystemIndex = systemIndex;
        }
    }
}
