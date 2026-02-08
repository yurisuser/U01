using System;
using System.IO;
using System.Text;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Ships;
using _Project.Scripts.Ships.Actions;
using _Project.Scripts.Ships.Orders;
using _Project.Scripts.Simulation.Ships;
using _Project.Scripts.Stations;
using UnityEngine;

namespace _Project.Scripts.Simulation.Global.Debug
{
    /// <summary>Отладочный пробник: пишет в файл трассу одного корабля TradeInSystem в глобальном пайплайне.</summary>
    public static class GlobalTradeDebugProbe
    {
        public static bool Enabled { get; set; } = true;
        public static int DebugShipUid { get; private set; }

        private static StreamWriter _writer;
        private static string _logPath;
        private static int _lastTurnDay = -1;

        public static void BeginTurn(int day, GameStateService gameState)
        {
            if (!Enabled)
                return; // Отладка выключена.

            EnsureWriter();
            if (_writer == null)
                return; // Не удалось открыть файл.

            if (_lastTurnDay != day)
            {
                _lastTurnDay = day;
                Log(day, -1, DebugShipUid, "Turn", "begin");
            }
        }

        public static void EndTurn()
        {
            if (!Enabled || _writer == null)
                return; // Нет активного writer.

            _writer.Flush();
        }

        public static void ResetSelection()
        {
            DebugShipUid = 0;
        }

        public static bool IsTrackedShip(in Ship ship)
        {
            return Enabled && DebugShipUid > 0 && ship.Uid.Id == DebugShipUid;
        }

        public static void LogShip(int day, int systemIndex, in Ship ship, string stage, string message)
        {
            if (!Enabled)
                return; // Отладка выключена.

            if (DebugShipUid <= 0)
            {
                DebugShipUid = ship.Uid.Id;
                Log(day, systemIndex, DebugShipUid, "Probe", "selected on first event");
            }

            if (!IsTrackedShip(in ship))
                return; // Логируем только выбранный корабль.

            Log(day, systemIndex, ship.Uid.Id, stage, message);
        }

        public static void Log(int day, int systemIndex, int shipUid, string stage, string message)
        {
            if (!Enabled || _writer == null)
                return; // Логирование выключено.

            var line = string.Format(
                "[GLOB-TRADE] day={0} sys={1} ship={2} stage={3} msg={4}",
                day,
                systemIndex,
                shipUid,
                stage,
                message);

            _writer.WriteLine(line);
        }

        public static string GetLogPath()
        {
            EnsureWriter();
            return _logPath ?? string.Empty;
        }

        private static void EnsureWriter()
        {
            if (_writer != null)
                return; // Writer уже готов.

            try
            {
                // В редакторе пишем прямо в проект, чтобы не искать файл в профиле пользователя.
                var baseDir = Application.dataPath;
                if (!string.IsNullOrWhiteSpace(baseDir))
                    baseDir = Path.Combine(baseDir, "_Project", "Logs");
                else
                    baseDir = Application.persistentDataPath;

                if (string.IsNullOrWhiteSpace(baseDir))
                    baseDir = ".";

                Directory.CreateDirectory(baseDir);
                _logPath = Path.Combine(baseDir, "global_trade_debug.log");
                _writer = new StreamWriter(_logPath, append: true, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _writer = null;
                UnityEngine.Debug.LogWarning("[GLOB-TRADE] failed to open log file: " + ex.Message);
            }
        }

    }
}
