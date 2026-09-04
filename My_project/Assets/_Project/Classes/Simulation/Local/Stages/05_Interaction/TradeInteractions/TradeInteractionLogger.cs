using _Project.Trade;
using _Project.Items;
using UnityEngine;

namespace _Project.Scripts.Simulation.Local.Stages.Interaction
{
    /// <summary>Единый формат debug-логов торговых interaction.</summary>
    internal static class TradeInteractionLogger
    {
        internal static void LogTradeStart(string action, int shipId, int stationId, ItemKey key, int amount, int price) // лог старта сделки
        {
            Debug.Log($"[Trade] {action} start. ship={shipId} station={stationId} item={key} amount={amount} price={price}"); // Точка начала сделки.
        }

        internal static void LogTradeFailed(string action, int shipId, int stationId, ItemKey key, int amount, ETradeFailReason reason) // лог ошибки сделки
        {
            Debug.Log($"[Trade] {action} failed. ship={shipId} station={stationId} item={key} amount={amount} reason={reason}"); // Причина отказа ExchangeService.
        }

        internal static void LogTradeOrderMissing(string side, int shipId, int stationId, ItemKey key) // лог отсутствующего ордера
        {
            Debug.Log($"[Trade] {side} order missing. ship={shipId} station={stationId} item={key}"); // Ордер удалился между планированием и исполнением.
        }

        internal static void LogTradeForcedSell(int shipId, int stationId, ItemKey key, int amount, int unitPrice) // forced-sell без buy-ордера
        {
            Debug.Log($"[Trade] forced sell. ship={shipId} station={stationId} item={key} amount={amount} price={unitPrice}");
        }
    }
}
