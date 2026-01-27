using _Project.CONST;
using _Project.DataAccess;

namespace _Project.Scripts.Stations
{
    /// <summary>Расчёт лимитов виртуального склада по базовой цене.</summary>
    public static class TradeLimits
    {
        public static int GetMaxAmount(TypeTradeItem itemKind, int itemId)
        {
            float basePrice = GetBasePrice(itemKind, itemId);
            if (basePrice <= 0f)
                return 0;

            return (int)(EconomyConstants.VirtualTradeBudget / basePrice);
        }

        private static float GetBasePrice(TypeTradeItem itemKind, int itemId)
        {
            switch (itemKind)
            {
                case TypeTradeItem.Goods:
                    if (CATALOG.GoodsById != null && CATALOG.GoodsById.TryGetValue(itemId, out var goods))
                        return goods.Price;
                    break;
                case TypeTradeItem.Sku:
                    if (CATALOG.SkuById != null && CATALOG.SkuById.TryGetValue(itemId, out var sku))
                        return sku.Price;
                    break;
            }

            return 0f;
        }
    }
}
