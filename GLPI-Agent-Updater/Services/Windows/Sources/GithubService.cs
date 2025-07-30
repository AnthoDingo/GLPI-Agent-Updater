using GLPIAgentUpdater.Interfaces;
using GLPIAgentUpdater.Interfaces.Windows;
using Octokit;
using System.Net;

namespace GLPIAgentUpdater.Services.Windows
{
    internal class GithubService : IChecker
    {
        private IEventManager _em;
        private readonly ILogger<GithubService> _logger;
        private readonly IRegistry _registry;
        private readonly IInstaller _installer;

        private readonly GitHubClient _client;
        private readonly int _interval;

        private Version _agentVersion;

        public GithubService(
            IEventManager eventManager,
            ILogger<GithubService> logger,
            IRegistry registry,
            IInstaller installer
        )
        {
            _em = eventManager;
            _logger = logger;
            _registry = registry;
            _installer = installer;

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


                Release onlineRelease = null;
                Version onlineVersion;
                if(((string)_registry.Get("Version")).Equals("Latest"))
                {
                    onlineRelease = releases[0];
                    
                } else
                {
                    onlineRelease = releases.FirstOrDefault(r => r.TagName.Equals((string)_registry.Get("Version")));
                }

                if(onlineRelease != null)
                {
                    Version.TryParse(onlineRelease.TagName, out onlineVersion);
                } else
                {
                    _em.Warning("Can't find online version from Github.");
                    await Task.Delay(_interval, cancellationToken);
                    continue;
                }
                

                if (onlineVersion == _agentVersion || onlineVersion < _agentVersion)
                {
                    _em.Info("GLPI Agent already up to date");
                    await Task.Delay(_interval, cancellationToken);
                    continue;
                }

                _em.Info($"New GLPI Agent available.\r\nCurrent installed version : {_agentVersion}\r\nAvailable version : {onlineVersion}", 10);
                await InstallFromGithubAsync(onlineRelease);

                await Task.Delay(_interval, cancellationToken);
            }
        }

        private async Task InstallFromGithubAsync(Release onlineRelease)
        {
            _em.Info($"Downloading GLPI Agent {onlineRelease.TagName} from Github");
            ReleaseAsset asset = onlineRelease.Assets.First(a => a.Name.Equals($"GLPI-Agent-{onlineRelease.TagName}-x64.msi"));

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

            await _installer.Install(filePath);
            _installer.CleanUp(filePath);
        }
    }
}
