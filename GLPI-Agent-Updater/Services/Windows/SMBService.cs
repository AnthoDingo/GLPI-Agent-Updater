using GLPIAgentUpdater.Services.Interfaces;
using Octokit;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.ServiceProcess;

namespace GLPIAgentUpdater.Services.Windows
{
    internal class SMBService : IChecker
    {
        private IEventManager _em;
        private readonly ILogger<SMBService> _logger;
        private readonly IRegistry _registry;
        private readonly GitHubClient _client;
        private readonly int _interval;

        private Version _agentVersion;

        public SMBService(
            IEventManager eventManager,
            ILogger<SMBService> logger,
            IRegistry registry)
        {
            _em = eventManager;
            _logger = logger;
            _registry = registry;

            _client = new GitHubClient(new ProductHeaderValue("GLPI-Agent-Updater"));
            try
            {
                _interval = (int)_registry.Get("CheckInterval") * 60 * 1000;
            }
            catch (Exception ex)
            {
                _interval = 60 * 60 * 1000;
            }
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {

                _agentVersion = _registry.GetAgentVersion();
                string? server = (string)_registry.Get("Server");
                if (server == null)
                {
                    _em.Error("Server cannot be empty");
                    throw new Exception("Server cannot be empty");
                }

                Uri uri = new Uri(server);
                if (Path.Exists(uri.AbsolutePath)) { 
                    
                }

                await Task.Delay(_interval, cancellationToken);
            }
        }

        private async Task InstallFromGithubAsync(Release latest)
        {
            _em.Info($"Downloading GLPI Agent {latest.TagName} from Github");
            ReleaseAsset asset = latest.Assets.First(a => a.Name.Equals($"GLPI-Agent-{latest.TagName}-x64.msi"));

            string filePath = Path.Join(Environment.GetEnvironmentVariable("TEMP"), asset.Name);
            Uri downloadUrl = new Uri(asset.BrowserDownloadUrl);

            try
            {
                WebClient webClient = new();
                webClient.DownloadFile(downloadUrl, filePath);
            }
            catch (WebException ex)
            {
                _em.Error(ex.Message, 1002);
                return;
            }

            FileInfo info = new FileInfo(filePath);
            if (info.Length != asset.Size)
            {
                _em.Warning($"GLPI Agent file size is incorrent.\r\nTarget value : {asset.Size}\r\nDownloaded file size : {info.Length}");
                return;
            }

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

                ServiceController service = new ServiceController("glpi-agent");
                service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(1));

                HttpClient httpClient = new HttpClient();
                await httpClient.GetAsync("http://127.0.0.1:62354/now");

            }
            catch (Win32Exception ex)
            {
                _em.Error($"Failed to install.\r\n{ex.Message}");
            }

            try
            {
                File.Delete(filePath);
            }
            catch (DirectoryNotFoundException ex)
            {

            }
            catch (IOException ex)
            {
                _em.Info($"Can't delete file {filePath}.\r\nFile is in used.\r\n{ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                _em.Info($"Can't delete file {filePath}.\r\nPermission denied.\r\n{ex.Message}");
            }
        }
    }
}
