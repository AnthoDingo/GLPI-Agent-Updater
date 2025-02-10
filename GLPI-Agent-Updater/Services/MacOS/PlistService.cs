using Claunia.PropertyList;
using GLPIAgentUpdater.Interfaces.MacOS;
using System.IO;

namespace GLPIAgentUpdater.Services.MacOS
{
    internal class PlistService : IPlist
    {
        private readonly ILogger<PlistService> _logger;

        private string _filePath = "/Library/Preferences/fr.frapps42.github-updater.plist";
        private FileInfo _fileInfo = new FileInfo("/Library/Preferences/fr.frapps42.github-updater.plist");
        
        private FileInfo _agentInfo = new FileInfo("/Applications/GLPI-Agent/Contents/Info.plist");

        public PlistService(ILogger<PlistService> logger){
            _logger = logger;

            if(!(File.Exists(_filePath))){
                NSDictionary root = new NSDictionary();
                
                root.Add("Mode", 0);
                root.Add("Version", "Latest");
                root.Add("Server", "");
                root.Add("CheckInterval", 120);
                
                PropertyListParser.SaveAsXml(root, new FileInfo(_filePath));
            }
        }
        
        public object? Get(string name){
            NSDictionary root = (NSDictionary)PropertyListParser.Parse(_fileInfo);
            try
            {
                NSObject obj = root.ObjectForKey(name);
                if (obj.GetType().Equals(typeof(NSNumber)))
                {
                    NSNumber num = (NSNumber)obj;
                    switch (num.GetNSNumberType())
                    {
                        case NSNumber.BOOLEAN:
                        {
                            return num.ToBool();
                            break;
                        }
                        case NSNumber.INTEGER:
                        {
                            return num.ToInt();
                            break;
                        }
                        case NSNumber.REAL:
                        {
                            return num.ToDouble();
                            break;
                        }
                    }
                }

                return obj.ToString();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Version? GetAgentVersion()
        {
            if (!File.Exists("/Applications/GLPI-Agent/Contents/Info.plist"))
            {
                Console.WriteLine("No GLPI Agent found");
                Environment.Exit(1);
            }

            Version agentVersion;
            NSDictionary root = (NSDictionary)PropertyListParser.Parse(_agentInfo);
            string value = root.ObjectForKey("CFBundleVersion").ToString();

            Version.TryParse(value, out agentVersion);
            
            return agentVersion;
        }
    }
}