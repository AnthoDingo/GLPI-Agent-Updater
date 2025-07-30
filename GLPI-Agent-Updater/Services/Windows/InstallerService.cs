using GLPIAgentUpdater.Interfaces;
using GLPIAgentUpdater.Interfaces.Windows;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;

namespace GLPIAgentUpdater.Services.Windows
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
            try
            {
                _em.Info($"Starting installation : {filePath}");
                Process process = new Process()
                {
                    StartInfo =
                    {
                        FileName = "msiexec.exe",
                        Arguments = $"/i \"{filePath}\" /qn"
                    }
                };

                process.Start();
                process.WaitForExit();
                try
                {
                    ServiceController service = new ServiceController("glpi-agent");
                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(1));

                    HttpClient httpClient = new HttpClient();
                    await httpClient.GetAsync("http://127.0.0.1:62354/now");
                } catch (Exception e)
                {
                    _em.Warning("Failed to start service GLPI-Agent");
                }
                

            }
            catch (Win32Exception ex)
            {
                _em.Error($"Failed to install.\r\n{ex.Message}");
            }
        }

        public void CleanUp(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (DirectoryNotFoundException ex)
            {

            }
            catch (IOException ex)
            {
                _em.Warning($"Can't delete file {filePath}.\r\nFile is in used.\r\n{ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                _em.Warning($"Can't delete file {filePath}.\r\nPermission denied.\r\n{ex.Message}");
            }
        }
    }
}
