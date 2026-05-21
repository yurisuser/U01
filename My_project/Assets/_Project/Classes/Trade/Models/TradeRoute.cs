using _Project.Scripts.Core;
using _Project.Items;

namespace _Project.Scripts.Trade.Models
{
    /// <summary>Маршрут сделки: где купить и где продать.</summary>
    public readonly struct TradeRoute
    {
        public readonly UID SellerUid;
        public readonly UID BuyerUid;
        public readonly ItemKey Key;
        public readonly int Amount;
        public readonly int SellPrice;
        public readonly int BuyPrice;

        public TradeRoute(UID sellerUid, UID buyerUid, ItemKey key, int amount, int sellPrice, int buyPrice)
        {
            SellerUid = sellerUid;
            BuyerUid = buyerUid;
            Key = key;
            Amount = amount;
            SellPrice = sellPrice;
            BuyPrice = buyPrice;
        }
    }
}
