using _Project.Items;

namespace _Project.Trade
{
    /// <summary>Торговые операции (перенос товара) без участия денег у NPC.</summary>
    public static class ExchangeService
    {
        /// <summary>Исполняет сделку: проверка, перенос товара, списание/начисление денег.</summary>
        public static TradeResult Execute(TradeOffer offer) // единая точка исполнения сделки
        {
            var check = CheckTradeService.Check(offer);
            if (!check.Success)
                return check;

            var sellerCargo = offer.Seller?.Cargo;
            var buyerCargo = offer.Buyer?.Cargo;
            if (sellerCargo == null || buyerCargo == null)
                return TradeResult.Fail(ETradeFailReason.InvalidInput);

            sellerCargo.Remove(offer.Key, offer.Amount);
            buyerCargo.Add(offer.Key, offer.Amount);

            ApplyMoney(offer);

            return TradeResult.Ok(offer.Amount);
        }

        /// <summary>Применяет оплату: всегда начисляем продавцу, у NPC- покупателя деньги безлимитные.</summary>
        private static void ApplyMoney(TradeOffer offer) // перенос денег между владельцами
        {
            var sellerOwner = offer.Seller?.Owner;
            var buyerOwner = offer.Buyer?.Owner;
            if (sellerOwner == null || buyerOwner == null)
                return;

            long total = (long)offer.UnitPrice * offer.Amount;
            if (total <= 0)
                return;

            buyerOwner.Money -= total;
            sellerOwner.Money += total;
        }
    }
}
