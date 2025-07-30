using GLPIAgentUpdater.Interfaces;
using GLPIAgentUpdater.Interfaces.Linux;
using Octokit;
using System.Net;

namespace GLPIAgentUpdater.Services.Linux.Sources
{
    internal class GithubService
    {
        private readonly IConfig _config;
        private readonly IInstaller _installer;

        private readonly GitHubClient _client;
        private readonly int _interval;

        private readonly PackagesService _packagesService;
        private Version _agentVersion;

        public GithubService(IConfig config, PackagesService packagesService, IInstaller installer)
        {
            _config = config;
            _packagesService = packagesService;
            _installer = installer;

            _client = new GitHubClient(new ProductHeaderValue("GLPI-Agent-Updater"));
            try
            {
                _interval = !String.IsNullOrEmpty(_config.GetValue("CheckInterval")) ? Convert.ToInt32(_config.GetValue("CheckInterval")) * 60 * 1000 : 60 * 60 * 1000;
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
                Console.WriteLine("Checking Github version");
                IReadOnlyList<Release> releases = await _client.Repository.Release.GetAll("glpi-project", "glpi-agent");

                if (releases.Count == 0)
                {
                    Console.WriteLine("No release available");
                    await Task.Delay(_interval, cancellationToken);
                    continue;
                }

                Release onlineRelease = null;
                Version onlineVersion;
                if (_config.GetValue("Version").Equals("Latest"))
                {
                    onlineRelease = releases[0];
                }
                else
                {
                    onlineRelease = !String.IsNullOrEmpty(_config.GetValue("Version")) ? releases.FirstOrDefault(r => r.TagName.Equals(_config.GetValue("Version"))) : releases[0];
                }

                if (onlineRelease != null)
                {
                    Version.TryParse(onlineRelease.TagName, out onlineVersion);
                } else
                {
                    Console.WriteLine("Can't find online version from Github.");
                    await Task.Delay(_interval, cancellationToken);
                    continue;
                }

                foreach (Models.Linux.Package package in _packagesService.GetInstalledAgents()) 
                {
                    Version agentVersion;
                    Version.TryParse(package.Version, out agentVersion);

                    if (onlineVersion == agentVersion || onlineVersion < agentVersion)
                    {
                        Console.WriteLine($"GLPI Agent {package.Name} already up to date");
                        await Task.Delay(_interval, cancellationToken);
                        continue;
                    }

                    Console.WriteLine($"New GLPI Agent {package.Name} available.\r\nCurrent installed version : {agentVersion}\r\nAvailable version : {onlineVersion}");

                }
            }   
        }

        private async Task InstallFromGithubAsync(Models.Linux.Package package, Release onlineRelease)
        {
            Console.WriteLine($"Downloading GLPI Agent {package.Name} {onlineRelease.TagName} from Github");
            ReleaseAsset asset = onlineRelease.Assets.First(a => a.Name.Equals($"{package.Name}-{onlineRelease.TagName}-1_all.deb"));

            string filePath = Path.Join("/tmp", asset.Name);
            Uri downloadUrl = new Uri(asset.BrowserDownloadUrl);

            try
            {
                WebClient webClient = new();
                webClient.DownloadFile(downloadUrl, filePath);
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            FileInfo info = new FileInfo(filePath);
            if (info.Length != asset.Size)
            {
                Console.WriteLine($"GLPI Agent {package.Name} file size is incorrent.\r\nTarget value : {asset.Size}\r\nDownloaded file size : {info.Length}");
                return;
            }

            await _installer.Install(filePath);
            _installer.CleanUp(filePath);
        }
    }
}
