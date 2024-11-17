using IniParser.Model;
using IniParser;
using GLPIAgentUpdater.Interfaces.Linux;

namespace GLPIAgentUpdater.Services.Linux
{
    internal class ConfigService : IConfig
    {
        private readonly string _filePath = "/etc/glpi-updater/updater.conf";

        private readonly FileIniDataParser _parser; 
        private KeyDataCollection _data;

        public ConfigService()
        {
            _parser = new FileIniDataParser();
        }

        private void LoadConfig() { 
            if (!File.Exists(_filePath)) { 
                Console.WriteLine($"File '{_filePath}' not found");

                string defaultConfig = @"
CheckInterval=120
Mode=0
Server=
Version=Latest
";

                try
                {
                    using (StreamWriter writer = new StreamWriter(_filePath))
                    {
                        writer.WriteLine(defaultConfig);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Canno't write default config file");
                }               
            } else
            {
                IniData iniData = _parser.ReadFile(_filePath);
                _data = iniData.Global;
            }             
        }

        public string? GetValue(string key)
        {
            try
            {
                LoadConfig();
                return _data[key];
            }
            catch
            {
                return null;
            }
            
        }
    }
}
