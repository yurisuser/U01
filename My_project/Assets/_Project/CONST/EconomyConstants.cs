namespace _Project.CONST
{
    /// <summary>Экономические константы проекта.</summary>
    public static class EconomyConstants
    {
        // виртуальный бюджет для определения лимитов склада по типу товара. VirtualTradeBudget/Base_price
        public const int VirtualTradeBudget = 100_000_000; 

        // фиктивные деньги для NPC (торговля не должна ограничивать их бюджетом)
        public const long NpcInfiniteMoney = long.MaxValue / 4;
        
    }
}
