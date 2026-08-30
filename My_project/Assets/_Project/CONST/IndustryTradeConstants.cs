namespace _Project.CONST
{
    /// <summary>Балансовые параметры автономной торговли индустриальных модулей.</summary>
    public static class IndustryTradeConstants
    {
        public const int InitialStockTurns = 3; // стартовый запас продукции в ходах
        public const int InputStockTurns = 3; // целевой запас входных ресурсов в ходах
        public const int OutputReserveTurns = 0; // запас выходных ресурсов, не выставляемый на продажу
        public const int MinOrderAmount = 1; // минимальный объём активного ордера
    }
}
