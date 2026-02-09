using _Project.Scripts.Trade.Models;
using UnityEngine;

namespace _Project.Scripts.Simulation.Local.Stages.Interaction
{
    /// <summary>Единый формат debug-логов торговых interaction.</summary>
    internal static class TradeInteractionLogger
    {
        internal static void LogTradeStart(string action, int shipId, int stationId, int itemId, int amount, int price) // лог старта сделки
        {
            Debug.Log($"[Trade] {action} start. ship={shipId} station={stationId} item={itemId} amount={amount} price={price}"); // Точка начала сделки.
        }

        internal static void LogTradeFailed(string action, int shipId, int stationId, int itemId, int amount, ETradeFailReason reason) // лог ошибки сделки
        {
            Debug.Log($"[Trade] {action} failed. ship={shipId} station={stationId} item={itemId} amount={amount} reason={reason}"); // Причина отказа TradeService.
        }

        internal static void LogTradeOrderMissing(string side, int shipId, int stationId, int itemId) // лог отсутствующего ордера
        {
            Debug.Log($"[Trade] {side} order missing. ship={shipId} station={stationId} item={itemId}"); // Ордер удалился между планированием и исполнением.
        }
    }
}
