using _Project.Scripts.Const;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using _Project.Scripts.Trade.Models;
using _Project.Scripts.Trade.Services;

namespace _Project.Scripts.Simulation.Ships
{
    /// <summary>Планирование цикла buy->sell внутри одной системы.</summary>
    internal static class TradeInSystemPlanner
    {
        public static void TryPlan(ref Ship ship, in StarSys system)
        {
            if (!ship.TaskState.HasTasks)
                TryBuildTaskStack(ref ship, in system); // Стек заполняем только когда он пуст.

            TradeDockActionAssigner.TryAssignFromTopMoveTask(ref ship); // Запрашиваем док при подходе к станции.
        }

        private static void TryBuildTaskStack(ref Ship ship, in StarSys system)
        {
            if (!SearchTradeService.TryFindBestInSystem(system, out TradeRoute route))
                return; // В системе нет прибыльного маршрута.

            if (!TradePlannerStationResolver.TryGetStation(in system, route.SellerUid, out var seller))
                return;
            if (!TradePlannerStationResolver.TryGetStation(in system, route.BuyerUid, out var buyer))
                return;

            int amount = FitToCargo(route.Amount, ship.Cargo.Capacity, ship.Cargo.Used);
            if (amount <= 0)
                return; // Нет свободного объема.

            // Важно: пушим в обратном порядке исполнения (LIFO стек).
            ship.TaskState.PushTask(ShipTaskBuilder.TradeSell(route.BuyerUid, route.ItemId, amount));
            ship.TaskState.PushTask(ShipTaskBuilder.MoveTo(
                buyer.Position,
                SimulationConsts.DestinationPointTolerance,
                keepSpeed: true,
                targetUid: route.BuyerUid));
            ship.TaskState.PushTask(ShipTaskBuilder.TradeBuy(route.SellerUid, route.ItemId, amount));
            ship.TaskState.PushTask(ShipTaskBuilder.MoveTo(
                seller.Position,
                SimulationConsts.DestinationPointTolerance,
                keepSpeed: true,
                targetUid: route.SellerUid));
        }

        private static int FitToCargo(int requestedAmount, int capacity, int used)
        {
            int amount = requestedAmount;
            if (capacity > 0)
            {
                int free = capacity - used;
                if (free <= 0)
                    return 0; // Трюм заполнен.

                if (amount > free)
                    amount = free; // Режем сделку по текущей вместимости.
            }

            return amount;
        }
    }
}
