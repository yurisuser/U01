using _Project.Items;

namespace _Project.Scripts.Trade.Models
{
    /// <summary>Данные сделки между двумя сторонами.</summary>
    public readonly struct TradeOffer
    {
        public readonly ITradeActor Seller;
        public readonly ITradeActor Buyer;
        public readonly ItemType ItemType;
        public readonly int ItemId;
        public readonly int Amount;
        public readonly int UnitPrice;
        public readonly int SystemIndex;

        public TradeOffer(
            ITradeActor seller,
            ITradeActor buyer,
            ItemType itemType,
            int itemId,
            int amount,
            int unitPrice,
            int systemIndex)
        {
            Seller = seller;
            Buyer = buyer;
            ItemType = itemType;
            ItemId = itemId;
            Amount = amount;
            UnitPrice = unitPrice;
            SystemIndex = systemIndex;
        }
    }
}
