using GLPIAgentUpdater.Interfaces;
using System.Diagnostics;

namespace GLPIAgentUpdater.Services.Windows
{
    internal class LogManager : IEventManager
    {
        private EventLog _log;

        public LogManager()
        {
            _log = new EventLog("Application");
            _log.Source = "GLPI Agent Updater";
        }

        public void Info(string message)
        {
            Info(message, 0);
        }

        public void Info(string message, int Id)
        {
            _log.WriteEntry(message, EventLogEntryType.Information, Id);
        }

        public void Error(string message)
        {
            Error(message, 0);
        }

        public void Error(string message, int Id)
        {
            _log.WriteEntry(message, EventLogEntryType.Error, Id);
        }

        public void Warning(string message)
        {
            Warning(message, 0);
        }

        public void Warning(string message, int Id)
        {
            _log.WriteEntry(message, EventLogEntryType.Warning, Id);
        }
    }
}
