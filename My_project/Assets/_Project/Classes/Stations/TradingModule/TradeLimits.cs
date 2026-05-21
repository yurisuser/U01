using _Project.CONST;
using _Project.DataAccess;
using _Project.Items;

namespace _Project.Scripts.Stations
{
    /// <summary>Расчёт лимитов виртуального склада по базовой цене.</summary>
    public static class TradeLimits
    {
        public static int GetMaxAmount(ItemKey key)
        {
            float basePrice = GetBasePrice(key);
            if (basePrice <= 0f)
                return 0;

            return (int)(EconomyConstants.VirtualTradeBudget / basePrice);
        }

        private static float GetBasePrice(ItemKey key)
        {
            if (ItemCatalogService.TryGetInfo(key.Type, key.Id, out var info))
                return info.Price;

            return 0f;
        }
    }
}
