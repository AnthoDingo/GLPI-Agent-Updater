using GLPIAgentUpdater.Interfaces;
using Octokit;
using System.Net;

namespace GLPIAgentUpdater.Services.Global 
{
    internal class GithubService: IChecker
    {
        private IEventManager _em;
        private readonly IConfig _config;
        private readonly GitHubClient _client;
        private readonly IInstaller _installer;
    
        private readonly int _interval;
        
        public GithubService(IEventManager em, IConfig config, IInstaller installer)
        {
            _em = em;
            _config = config;
            _client = new GitHubClient(new ProductHeaderValue("GLPI-Agent-Updater"));
            _installer = installer;
            
            try
            {
                _interval = (int)_config.Get("CheckInterval") * 60 * 1000;
            }
            catch (Exception ex)
            {
                _interval = 60 * 60 * 1000;
            }
        }

        #region Runner

        public async Task Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _em.Info("Checking Github version");

                Release onlineRelease = null;
                if (((string)_config.Get("Version")).Equals("Latest"))
                {
                    onlineRelease = await this.GetLatest();

                }
                else
                {
                    onlineRelease = await this.GetTarget((string)_config.Get("Version"));
                }

                if (onlineRelease == null)
                {
                    _em.Warning("Can't find online version from Github.");
                    await Task.Delay(_interval, cancellationToken);
                    continue;
                }

                Version onlineVersion;
                Version.TryParse(onlineRelease.TagName, out onlineVersion);
                Version agentVersion = _config.GetAgentVersion();
                if (onlineVersion == agentVersion || onlineVersion < agentVersion)
                {
                    _em.Info("GLPI Agent already up to date");
                    await Task.Delay(_interval, cancellationToken);
                    continue;
                }

                _em.Info(
                    $"New GLPI Agent available.\r\nCurrent installed version : {agentVersion}.\r\nAvailable version : {onlineVersion}");
                try
                {
                    string filePath = await this.DownloadRelease(onlineRelease, Path.GetTempPath());
                    _em.Info("Installing new version");
                    await _installer.Install(filePath);
                    _installer.CleanUp(filePath);
                }
                catch (Exception ex)
                {
                    _em.Error($"Failed to install version {onlineRelease.TagName}");
                }

                await Task.Delay(_interval, cancellationToken);
            }
        }

        #endregion
        
        #region Functions to get release

        internal async Task<IReadOnlyList<Release>> GetAvailableVersion()
        {
            IReadOnlyList<Release> releases = null;
            try
            {
                releases = await _client.Repository.Release.GetAll("glpi-project", "glpi-agent");
                if (releases.Count == 0)
                {
                    _em.Warning("No release available from Github");
                }
            }
            catch (Exception ex)
            {
                _em.Error(ex.Message);
            }
            
            return releases;
        }

        internal async Task<Release> GetLatest()
        {
            IReadOnlyList<Release> releases = await GetAvailableVersion();
            return GetLatest(releases);
        }
        internal Release GetLatest(IReadOnlyList<Release> releases)
        {
            if (releases.Count > 0)
            {
                return releases[0];
            }
            else
            {
                return null;
            }
        }

        internal async Task<Release> GetTarget(string target)
        {
            IReadOnlyList<Release> releases = await GetAvailableVersion();
            return GetTarget(releases, target);
        }
        
        internal Release GetTarget(IReadOnlyList<Release> releases, string target)
        {
            Version targetVersion;
            Version.TryParse(target, out targetVersion);
            return GetTarget(releases, targetVersion);
        }

        internal async Task<Release> GetTarget(Version target)
        {
            IReadOnlyList<Release> releases = await GetAvailableVersion();
            return GetTarget(releases, target);
        }
        internal Release GetTarget(IReadOnlyList<Release> releases, Version target)
        {
            return releases.FirstOrDefault(r => r.TagName.Equals(target.ToString()));
        }

        internal async Task<string> DownloadRelease(Release release, string path)
        {
            _em.Info($"Downloading GLPI Agent {release.TagName} from Github");

            string assetFilter = null;
            #if OS_WINDOWS
            assetFilter = $"GLPI-Agent-{release.TagName}-x64.msi";
            #endif
            
            #if OS_MAC
            assetFilter = $"GLPI-Agent-{release.TagName}_x86_64.pkg";
            #endif
            
            #if OS_LINUX
            
            #endif
            
            ReleaseAsset asset = release.Assets.First(a => a.Name.Equals(assetFilter));
            if (asset == null)
            {
                throw new NotImplementedException();
            }
            
            string filePath = Path.Join(path, asset.Name);
            Uri downloadUrl = new Uri(asset.BrowserDownloadUrl);
            
            try
            {
                WebClient webClient = new();
                webClient.DownloadFile(downloadUrl, filePath);
            }
            catch (WebException ex)
            {
                _em.Error(ex.Message, 1002);
                return null;
            }
            
            FileInfo info = new FileInfo(filePath);
            if (info.Length != asset.Size)
            {
                _em.Warning($"GLPI Agent file size is not correct.\r\nTarget value : {asset.Size}\r\nDownloaded file size : {info.Length}");
                return null;
            }
            else
            {
                _em.Info("Download is correct");
            }

            return filePath;
        }

        #endregion
        
    }
}