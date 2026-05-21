using _Project.Items;
using _Project.Scripts.Trade.Models;

namespace _Project.Scripts.Trade.Services
{
    public static class CheckTradeService
    {
        /// <summary>Главная проверка сделки: тонкий оркестратор пайплайна.</summary>
        public static TradeResult Check(TradeOffer offer) // общий вход проверки сделки
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

        private static TradeResult CheckOffer(TradeOffer offer) // базовая валидность входа
        {
            if (offer.Seller == null || offer.Buyer == null)
                return TradeResult.Fail(ETradeFailReason.InvalidInput);

            if (offer.Amount <= 0 || offer.Key.IsEmpty)
                return TradeResult.Fail(ETradeFailReason.InvalidInput);

            if (offer.UnitPrice < 0)
                return TradeResult.Fail(ETradeFailReason.InvalidInput);

            return TradeResult.Ok(offer.Amount);
        }

        private static TradeResult CheckSellerStock(TradeOffer offer) // хватает ли товара у продавца
        {
            var cargo = offer.Seller.Cargo;
            if (cargo == null)
                return TradeResult.Fail(ETradeFailReason.NotEnoughStock);

            int have = cargo.GetAmount(offer.Key);
            if (have < offer.Amount)
                return TradeResult.Fail(ETradeFailReason.NotEnoughStock);

            return TradeResult.Ok(offer.Amount);
        }

        private static TradeResult CheckBuyerCapacity(TradeOffer offer) // есть ли место у покупателя
        {
            var cargo = offer.Buyer.Cargo;
            if (cargo == null)
                return TradeResult.Fail(ETradeFailReason.NotEnoughCargoSpace);

            if (!cargo.CanAdd(offer.Key, offer.Amount))
                return TradeResult.Fail(ETradeFailReason.NotEnoughCargoSpace);

            return TradeResult.Ok(offer.Amount);
        }

        /// <summary>Проверяет, что у покупателя хватает денег.</summary>
        private static TradeResult CheckBuyerMoney(TradeOffer offer) // проверка денег покупателя
        {
            if (offer.UnitPrice < 0 || offer.Amount <= 0)
                return TradeResult.Fail(ETradeFailReason.InvalidInput);

            long total = (long)offer.UnitPrice * offer.Amount;
            if (offer.Buyer.Money < total)
                return TradeResult.Fail(ETradeFailReason.NotEnougMoney);

            return TradeResult.Ok(offer.Amount);
        }
    }
}
