using GLPIAgentUpdater.Interfaces;
using System.Globalization;

namespace GLPIAgentUpdater.Services.MacOS
{
    public class LogManager : IEventManager
    {
        private readonly string _logPath = "/private/var/log/glpi-updater.log";
        private object lockObj = new object();
        
        public LogManager()
        {
            if (!File.Exists(_logPath))
            {
                File.Create(_logPath);
            }
        }

        private void WriteValue(string level, string message)
        {
            string timestamp = DateTime.Now.ToString("ddd MMM d HH:mm:ss yyyy", CultureInfo.InvariantCulture);
            string logEntry = $"[{timestamp}][{level.ToLower()}] - {message}";
            lock (lockObj)
            {
                try
                {
                    File.AppendAllText(_logPath, logEntry + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    // Handle any errors that might occur when writing to the log file
                    Console.WriteLine($"Failed to write to log file: {ex.Message}");
                }
            }
        }
        
        public void Info(string message)
        {
            WriteValue("info", message);
        }

        public void Info(string message, int Id)
        {
            Info(message);
        }

        public void Error(string message)
        {
            WriteValue("error", message);
        }

        public void Error(string message, int Id)
        {
            Error(message);
        }

        public void Warning(string message)
        {
            WriteValue("warning", message);
        }

        public void Warning(string message, int Id)
        {
            Warning(message);
        }
    }
}
    