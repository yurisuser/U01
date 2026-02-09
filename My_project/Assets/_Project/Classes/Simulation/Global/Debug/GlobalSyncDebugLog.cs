using System;
using System.IO;
using System.Text;
using System.Threading;

namespace _Project.Scripts.Simulation.Global.Debug
{
    /// <summary>Потокобезопасный файловый лог для диагностики handshake и работы global worker.</summary>
    public static class GlobalSyncDebugLog
    {
        public static bool Enabled { get; set; } = true;

        private static readonly object _sync = new object();
        private static StreamWriter _writer;
        private static string _logPath;

        public static void Log(string source, string message)
        {
            if (!Enabled)
                return;

            lock (_sync)
            {
                EnsureWriter();
                if (_writer == null)
                    return;

                var now = DateTime.UtcNow.ToString("O");
                int tid = Thread.CurrentThread.ManagedThreadId;
                _writer.WriteLine("[GLOB-SYNC] ts=" + now + " tid=" + tid + " src=" + source + " msg=" + message);
                _writer.Flush();
            }
        }

        public static string GetLogPath()
        {
            lock (_sync)
            {
                EnsureWriter();
                return _logPath ?? string.Empty;
            }
        }

        private static void EnsureWriter()
        {
            if (_writer != null)
                return;

            try
            {
                // Важно: пишем вне Assets, чтобы не провоцировать Unity reimport во время playmode.
                var dir = Path.Combine(Path.GetTempPath(), "U01", "Logs");
                Directory.CreateDirectory(dir);
                _logPath = Path.Combine(dir, "global_sync_debug.log");
                _writer = new StreamWriter(_logPath, append: true, Encoding.UTF8);
            }
            catch
            {
                _writer = null;
            }
        }
    }
}
