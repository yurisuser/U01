using _Project.CONST;
using _Project.DataAccess;

namespace _Project.Scripts.Stations
{
    /// <summary>Расчёт лимитов виртуального склада по базовой цене.</summary>
    public static class TradeLimits
    {
        public static int GetMaxAmount(_Project.Items.ItemType type, int itemId)
        {
            float basePrice = GetBasePrice(type, itemId);
            if (basePrice <= 0f)
                return 0;

            return (int)(EconomyConstants.VirtualTradeBudget / basePrice);
        }

        private static float GetBasePrice(_Project.Items.ItemType type, int itemId)
        {
            if (CATALOG.ItemsById != null && CATALOG.ItemsById.TryGetValue(itemId, out var item))
                return item.Price;

            return 0f;
        }
    }
}
