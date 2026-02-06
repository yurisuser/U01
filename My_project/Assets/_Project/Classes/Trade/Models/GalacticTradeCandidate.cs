using _Project.Scripts.Core;

namespace _Project.Scripts.Trade.Models
{
    /// <summary>Кандидат межсистемной сделки с данными для маршрутизации.</summary>
    public readonly struct GalacticTradeCandidate
    {
        public readonly UID SellerUid;          // станция-продавец
        public readonly int SellerSystemIndex;  // система продавца
        public readonly UID BuyerUid;           // станция-покупатель
        public readonly int BuyerSystemIndex;   // система покупателя
        public readonly int ItemId;
        public readonly int Amount;
        public readonly int SellPrice;          // цена продажи (у покупателя)
        public readonly int BuyPrice;           // цена покупки (у продавца)
        public readonly int HopsToSeller;       // прыжков до продавца
        public readonly int HopsSellerToBuyer;  // прыжков от продавца до покупателя

        public GalacticTradeCandidate(
            UID sellerUid,
            int sellerSystemIndex,
            UID buyerUid,
            int buyerSystemIndex,
            int itemId,
            int amount,
            int sellPrice,
            int buyPrice,
            int hopsToSeller,
            int hopsSellerToBuyer)
        {
            SellerUid = sellerUid;
            SellerSystemIndex = sellerSystemIndex;
            BuyerUid = buyerUid;
            BuyerSystemIndex = buyerSystemIndex;
            ItemId = itemId;
            Amount = amount;
            SellPrice = sellPrice;
            BuyPrice = buyPrice;
            HopsToSeller = hopsToSeller;
            HopsSellerToBuyer = hopsSellerToBuyer;
        }

        public int Profit => (SellPrice - BuyPrice) * Amount;
    }
}
