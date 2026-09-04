using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using _Project.Scripts.Ships.Actions;
using _Project.Scripts.Simulation.Ships;
using _Project.Scripts.Stations;
using _Project.Trade;
using UnityEngine;

namespace _Project.Scripts.Simulation.Local.Stages.Interaction
{
    /// <summary>Общий проход по докнутым кораблям с торговыми action.</summary>
    internal static class TradeInteraction
    {
        public static void ProcessTradeActions(ref StarSys system) // торговые действия докнутых кораблей в системе
        {
            var stations = system.Stations;
            if (stations == null || stations.Length == 0) // нет станций — нечего обрабатывать
                return;

            for (int s = 0; s < stations.Length; s++) // перебор станций
            {
                var station = stations[s];
                if (!TryGetDockState(in station, out var dockState))
                    continue; // На станции нет dock-модуля.

                var docked = dockState.DockedShips;
                for (int i = 0; i < docked.Count; i++) // перебор докнутых кораблей
                {
                    var ship = docked[i];
                    if (ship.CurrentAction.Type != EShipActionType.TradeBuy &&
                        ship.CurrentAction.Type != EShipActionType.TradeSell) // фильтр: только торговые действия
                        continue;

                    if (!TryGetStation(in system, ship.CurrentAction.TargetUid, out var targetStation)) // ищем целевую станцию
                    {
                        ship.CurrentAction = default; // Сбрасываем действие, цель недоступна.
                        ship.LastActionFailReason = EShipActionFailReason.TargetNotFound; // Для дебага/аналитики причины.
                        docked[i] = ship;
                        continue;
                    }

                    if (!TryGetTradeState(in targetStation, out var tradeState)) // требуется торговый модуль
                    {
                        ship.CurrentAction = default; // Целевая станция найдена, но торговать нельзя.
                        ship.LastActionFailReason = EShipActionFailReason.TargetNotFound;
                        docked[i] = ship;
                        continue;
                    }

                    if (ship.CurrentAction.Type == EShipActionType.TradeBuy) // ветка покупки
                        TradeBuyInteraction.Process(ref ship, ref system, in targetStation, tradeState);
                    else if (ship.CurrentAction.Type == EShipActionType.TradeSell) // ветка продажи
                        TradeSellInteraction.Process(ref ship, ref system, in targetStation, tradeState);

                    docked[i] = ship;
                }
            }
        }

        internal static bool TryGetTradeState(in Station station, out TradeModuleState tradeState) // поиск торгового модуля станции
        {
            var modules = station.Modules;
            if (modules != null) // есть модули станции
            {
                for (int i = 0; i < modules.Length; i++) // перебор модулей
                {
                    var module = modules[i];
                    if (module == null || module.Type != EStationModuleType.Trade)
                        continue;

                    tradeState = module.State as TradeModuleState; // Берем runtime-состояние trade-модуля.
                    return tradeState != null;
                }
            }

            tradeState = null; // На станции нет рабочего trade-модуля.
            return false;
        }

        internal static bool TryGetStation(in StarSys system, UID stationUid, out Station station) // поиск станции по UID
        {
            var stations = system.Stations;
            for (int i = 0; i < stations.Length; i++) // перебор станций
            {
                if (stations[i].Uid.Id == stationUid.Id) // совпадение по UID
                {
                    station = stations[i]; // Возвращаем value-type станции из массива системы.
                    return true;
                }
            }

            station = default;
            return false;
        }

        private static bool TryGetDockState(in Station station, out DockModuleState dockState)
        {
            var modules = station.Modules;
            if (modules == null)
            {
                dockState = null; // Нет модулей вообще.
                return false;
            }

            for (int i = 0; i < modules.Length; i++)
            {
                var module = modules[i];
                if (module == null || module.Type != EStationModuleType.Dock)
                    continue;

                dockState = module.State as DockModuleState; // В dock-состоянии лежит список DockedShips.
                return dockState != null;
            }

            dockState = null; // На станции нет dock-модуля.
            return false;
        }
    }
}
