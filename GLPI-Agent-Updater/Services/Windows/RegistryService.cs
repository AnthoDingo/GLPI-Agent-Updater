using GLPIAgentUpdater.Services.Interfaces;
using Microsoft.Win32;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace GLPIAgentUpdater.Services.Windows
{
    internal class RegistryService : IRegistry
    {
        private IEventManager _em;
        private readonly ILogger<RegistryService> _logger;

        private string _keyName = @"SOFTWARE\GLPI-Agent-Updater";
        RegistryKey _key;

        public RegistryService(IEventManager eventManager, ILogger<RegistryService> logger)
        {
            _em = eventManager;
            _logger = logger;

            if (Registry.LocalMachine.OpenSubKey(_keyName) == null)
            {
                try
                {
                    _key = Registry.LocalMachine.CreateSubKey(_keyName, RegistryKeyPermissionCheck.ReadWriteSubTree);
                }
                catch (Exception ex)
                {
                    Log(ex.Message);
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                _key = Registry.LocalMachine.OpenSubKey(_keyName, true);
            }

            if (Get("Github") == null)
            {
                _key.SetValue("Github", true, RegistryValueKind.DWord);
            }
            if (Get("GithubDelay") == null)
            {
                _key.SetValue("GithubDelay", 0, RegistryValueKind.DWord);
            }

            if (Get("Server") == null)
            {
                _key.SetValue("Server", string.Empty, RegistryValueKind.String);
            }

            if (Get("CheckInterval") == null)
            {
                _key.SetValue("CheckInterval", 120, RegistryValueKind.DWord);
            }
        }

        private void Log(string message)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(message);
            }
        }

        public object? Get(string name)
        {
            return _key.GetValue(name);
        }

        public object? Get(string key, string name)
        {
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(key))
            {
                return registryKey.GetValue(name);
            }
        }

        public Version? GetAgentVersion()
        {
            if (!File.Exists($@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\GLPI-Agent\glpi-agent.bat"))
            {
                _em.Error("No GLPI Agent found", 1000);
                Environment.Exit(1);
            }

            Version agentVersion;
            RegistryKey uninstallKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            foreach (string subKeyName in uninstallKey.GetSubKeyNames())
            {
                RegistryKey subKey = uninstallKey.OpenSubKey(subKeyName);
                object objDisplayName = subKey.GetValue("DisplayName");
                if (objDisplayName == null)
                {
                    continue;
                }

                string displayName = objDisplayName.ToString();
                if (Regex.IsMatch(displayName, @"^GLPI Agent \d+\.\d+\.\d+$"))
                {
                    Version.TryParse(subKey.GetValue("DisplayVersion").ToString(), out agentVersion);
                    return agentVersion;
                }
            }

            return null;
        }
    }
}
