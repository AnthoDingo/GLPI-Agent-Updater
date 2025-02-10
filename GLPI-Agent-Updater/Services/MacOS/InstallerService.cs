using System.Diagnostics;
using GLPIAgentUpdater.Enums;
using GLPIAgentUpdater.Interfaces;
using Octokit;

namespace GLPIAgentUpdater.Services.MacOS
{
    internal class InstallerService : IInstaller
    {
        private IEventManager _em;
        
        public InstallerService(IEventManager eventManager)
        {
            _em = eventManager;
        }
        
        public async Task Install(string filePath)
        {
            InvokeServiceAction(ServiceAction.stop);

            Dictionary<string, string> configs;
            
            // Backup configs
            configs = BackupFiles("/Applications/GLPI-Agent/etc/conf.d");
            
            try
            {
                runProcess("installer", $@"-pkg {filePath} -target /");
            }
            catch (Exception ex)
            {
                _em.Error("Failed to install glpi-agent.");
            }
            
            // Restore configs
            RestoreFiles("/Applications/GLPI-Agent/etc/conf.d", configs);
            
            InvokeServiceAction(ServiceAction.start);

            try
            {
                Thread.Sleep(15000);
                HttpClient httpClient = new HttpClient();
                await httpClient.GetAsync("http://127.0.0.1:62354/now");
            }
            catch (Exception ex)
            {
                _em.Warning("Failed to run task right now.");
            }
        }

        public void CleanUp(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (FileNotFoundException ex)
            {
                _em.Warning($"File not found {filePath}");
            }
            catch (IOException ex)
            {
                _em.Warning($"Can't delete file {filePath}. File is in used.\r\n{ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                _em.Warning($"Can't delete file {filePath}. Permission denied.\r\n{ex.Message}");
            }
        }

        private void runProcess(string app, string args)
        {
            Process process = new Process()
            {
                StartInfo =
                {
                    FileName = app,
                    Arguments = args
                }
            };
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception("App error");
            }
        }

        private void InvokeServiceAction(ServiceAction action)
        {
            try
            {
                runProcess("launchctl", $"{action.ToString()} com.teclib.glpi-agent");
            }
            catch (Exception ex)
            {
                _em.Error($"Failed to {action.ToString()} glpi-agent service.");
                return;
            }
        }
        
        private Dictionary<string, string> BackupFiles(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException();
            }
            
            Dictionary<string, string> files = new Dictionary<string, string>();
            
            string[] filesCfg = Directory.GetFiles(path);
            foreach (string file in filesCfg)
            {
                try
                {
                    // Lire le contenu du fichier
                    string content = File.ReadAllText(file);

                    // Ajouter le fichier et son contenu au dictionnaire
                    files.Add(Path.GetFileName(file), content);
                }
                catch (Exception ex)
                {
                    _em.Error($"Failed to backup file {file} : {ex.Message}");
                    throw new Exception($"Failed to backup file {file} : {ex.Message}");
                }
            }

            return files;
        }

        private void RestoreFiles(string path, Dictionary<string, string> files)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException();
            }
            
            foreach (KeyValuePair<string, string> file in files)
            {
                string filePath = Path.Combine(path, file.Key);
                try
                {
                    File.WriteAllText(filePath, file.Value);
                }
                catch (Exception ex)
                {
                    _em.Error($"Failed to restore file {file.Key} : {ex.Message}");
                    throw new Exception($"Failed to restore file {file.Key} : {ex.Message}");
                }   
            }
        }
    }
}