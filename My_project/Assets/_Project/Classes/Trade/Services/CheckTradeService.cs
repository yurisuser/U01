using _Project.Items;
using _Project.Scripts.NPC.Fraction;
using _Project.Scripts.Trade.Models;

namespace _Project.Scripts.Trade.Services
{
    public static class CheckTradeService
    {
        /// <summary>Главная проверка сделки: тонкий оркестратор пайплайна.</summary>
        public static TradeResult Check(TradeOffer offer)
        {
            var result = CheckOffer(offer); //Базовая валидация входных данных сделки.
            if (!result.Success)
                return result;

            result = CheckSellerStock(offer); //>Проверяет, что у продавца хватает товара
            if (!result.Success)
                return result;

            result = CheckBuyerCapacity(offer); //Проверяет, что у покупателя есть место под груз.
            if (!result.Success)
                return result;

            result = CheckBuyerMoney(offer); //Проверяет, что у покупателя хватает денег.
            if (!result.Success)
                return result;

            return TradeResult.Ok(offer.Amount);
        }

        private static TradeResult CheckOffer(TradeOffer offer)
        {
            if (offer.Seller == null || offer.Buyer == null)
                return TradeResult.Fail(ETradeFailReason.InvalidInput);

            if (offer.Amount <= 0 || offer.ItemId <= 0)
                return TradeResult.Fail(ETradeFailReason.InvalidInput);

            if (offer.UnitPrice < 0)
                return TradeResult.Fail(ETradeFailReason.InvalidInput);

            return TradeResult.Ok(offer.Amount);
        }

        private static TradeResult CheckSellerStock(TradeOffer offer)
        {
            var cargo = offer.Seller.Cargo;
            if (cargo == null)
                return TradeResult.Fail(ETradeFailReason.NotEnoughStock);

            int have = cargo.GetAmount(offer.ItemType, offer.ItemId);
            if (have < offer.Amount)
                return TradeResult.Fail(ETradeFailReason.NotEnoughStock);

            return TradeResult.Ok(offer.Amount);
        }

        private static TradeResult CheckBuyerCapacity(TradeOffer offer)
        {
            var cargo = offer.Buyer.Cargo;
            if (cargo == null)
                return TradeResult.Fail(ETradeFailReason.NotEnoughCargoSpace);

            if (!cargo.CanAdd(offer.ItemType, offer.ItemId, offer.Amount))
                return TradeResult.Fail(ETradeFailReason.NotEnoughCargoSpace);

            return TradeResult.Ok(offer.Amount);
        }

        /// <summary>Проверяет, что у покупателя хватает денег.</summary>
        private static TradeResult CheckBuyerMoney(TradeOffer offer)
        {
            if (offer.UnitPrice < 0 || offer.Amount <= 0)
                return TradeResult.Fail(ETradeFailReason.InvalidInput);

            if (offer.Buyer?.Owner != null && offer.Buyer.Owner.FractionType != EFractionTypes.Player)
                return TradeResult.Ok(offer.Amount);

            long total = (long)offer.UnitPrice * offer.Amount;
            if (offer.Buyer.Money < total)
                return TradeResult.Fail(ETradeFailReason.NotEnougMoney);

            return TradeResult.Ok(offer.Amount);
        }
    }
}
