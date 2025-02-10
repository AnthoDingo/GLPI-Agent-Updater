using GLPIAgentUpdater.Interfaces;
using Octokit;
using System.Net;
using GLPIAgentUpdater.Enums;

namespace GLPIAgentUpdater.Services.Shared
{
    internal class GithubService
    {
        private IEventManager _em;
        private readonly GitHubClient _client;
    
        public GithubService(IEventManager em)
        {
            _em = em;
            _client = new GitHubClient(new ProductHeaderValue("GLPI-Agent-Updater"));
        }

        internal async Task<IReadOnlyList<Release>> GetAvailableVersion()
        {
            IReadOnlyList<Release> releases = await _client.Repository.Release.GetAll("glpi-project", "glpi-agent");
            if (releases.Count == 0)
            {
                _em.Warning("No release available from Github");
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
            if (releases.Count == 0)
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

        internal async Task<string> DownloadRelease(Release release, string path, Extension extension)
        {
            _em.Info($"Downloading GLPI Agent {release.TagName} from Github");

            ReleaseAsset asset = null;
            switch (extension)
            {
                default:
                case Extension.msi:
                    
                    break;
                case Extension.pkg:
                    asset = release.Assets.First(a => a.Name.Equals($"GLPI-Agent-{release.TagName}_x86_64.pkg"));
                    break;
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
                _em.Warning($"GLPI Agent file size is not correct. Target value : {asset.Size} Downloaded file size : {info.Length}");
                return null;
            }
            else
            {
                _em.Info("Download is correct");
            }

            return filePath;
        }
    }
}