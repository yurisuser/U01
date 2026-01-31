using _Project.Scripts.Core;

namespace _Project.Scripts.Trade.Models
{
    /// <summary>Маршрут сделки: где купить и где продать.</summary>
    public readonly struct TradeRoute
    {
        public readonly UID SellerUid;
        public readonly UID BuyerUid;
        public readonly int ItemId;
        public readonly int Amount;
        public readonly int SellPrice;
        public readonly int BuyPrice;

        public TradeRoute(UID sellerUid, UID buyerUid, int itemId, int amount, int sellPrice, int buyPrice)
        {
            SellerUid = sellerUid;
            BuyerUid = buyerUid;
            ItemId = itemId;
            Amount = amount;
            SellPrice = sellPrice;
            BuyPrice = buyPrice;
        }
    }
}
