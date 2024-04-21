using GLPIAgentUpdater.Services.Interfaces;
using Octokit;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.ServiceProcess;

namespace GLPIAgentUpdater.Services.Windows
{
    internal class GithubService : IChecker
    {
        private IEventManager _em;
        private readonly ILogger<GithubService> _logger;
        private readonly IRegistry _registry;
        private readonly GitHubClient _client;
        private readonly int _interval;

        private Version _agentVersion;

        public GithubService(
            IEventManager eventManager,
            ILogger<GithubService> logger,
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

                _em.Info("Checking Github version");
                IReadOnlyList<Release> releases = await _client.Repository.Release.GetAll("glpi-project", "glpi-agent");

                if (releases.Count == 0)
                {
                    _em.Info("No release available");
                    await Task.Delay(_interval, cancellationToken);
                    continue;
                }

                Release latest = releases[0];
                Version latestVersion;
                Version.TryParse(latest.TagName, out latestVersion);

                if (latestVersion == _agentVersion || latestVersion < _agentVersion)
                {
                    _em.Info("GLPI Agent already up to date");
                    await Task.Delay(_interval, cancellationToken);
                    continue;
                }

                _em.Info($"New GLPI Agent available.\r\nCurrent installed version : {_agentVersion}\r\nAvailable version : {latestVersion}", 10);


                int delay = (int)_registry.Get("GithubDelay");

                // No delayed installation
                if (delay == 0)
                {
                    await InstallFromGithubAsync(latest);
                    await Task.Delay(_interval, cancellationToken);
                    continue;
                }

                DateTimeOffset now = new DateTimeOffset(DateTime.Now);
                TimeSpan delta = (TimeSpan)(now - latest.PublishedAt);
                if (delta.Days > delay)
                {
                    _em.Info("Installation delayed is done.");
                    await InstallFromGithubAsync(latest);
                }
                else
                {
                    _em.Info($"Installation delayed. Days befor update : {delta.Days}");
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
