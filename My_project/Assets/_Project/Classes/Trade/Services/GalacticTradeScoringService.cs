using _Project.Scripts.Const;
using _Project.Scripts.Trade.Models;

namespace _Project.Scripts.Trade.Services
{
    /// <summary>Оценка межсистемных маршрутов без жёстких порогов: приоритет ближних и прибыльных.</summary>
    public static class GalacticTradeScoringService
    {
        /// <summary>Считает время рейса: до продавца + до покупателя (прыжки + подлёты).</summary>
        public static float EstimateTravelTurns(in GalacticTradeCandidate route)
        {
            int hopsToSeller = route.HopsToSeller < 0 ? 0 : route.HopsToSeller;
            int hopsSellerToBuyer = route.HopsSellerToBuyer < 0 ? 0 : route.HopsSellerToBuyer;

            float jumpTurns = (hopsToSeller + hopsSellerToBuyer) * ContinuumConsts.JumpDurationTurns;
            // подлёты в двух системах: к зоне/станции продавца и покупателя
            float approach = ContinuumConsts.ApproachTurns * 2;
            return jumpTurns + approach;
        }

        /// <summary>Считает yield = profit/turns. Если turns == 0, возвращает 0.</summary>
        public static float ComputeYieldPerTurn(in GalacticTradeCandidate route)
        {
            float turns = EstimateTravelTurns(in route);
            if (turns <= 0f)
                return 0f;
            return route.Profit / turns;
        }

        /// <summary>Считает относительный рейтинг к среднему yield (без порогов).</summary>
        public static float ComputeScore(in GalacticTradeCandidate route, float avgYield)
        {
            float yield = ComputeYieldPerTurn(in route);
            if (avgYield <= 0f)
                return yield; // если истории нет — сортируем по yield
            return yield / avgYield;
        }
    }
}
