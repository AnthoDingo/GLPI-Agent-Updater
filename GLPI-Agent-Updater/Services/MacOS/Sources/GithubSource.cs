using GLPIAgentUpdater.Enums;
using GLPIAgentUpdater.Interfaces;
using GLPIAgentUpdater.Interfaces.MacOS;
using GLPIAgentUpdater.Services.Shared;
using Octokit;

namespace GLPIAgentUpdater.Services.MacOS
{
    internal class GithubSource : IChecker
    {
        private IEventManager _em;
        private readonly IPlist _plist;
        private readonly GithubService _github;
        private readonly IInstaller _installer;
        
        private readonly int _interval;

        public GithubSource
        (
            IEventManager eventManager,
            IPlist plist,
            GithubService github,
            IInstaller installer
        )
        {
            _em = eventManager;
            _plist = plist;
            _github = github;
            _installer = installer;
            
            try
            {
                _interval = (int)_plist.Get("CheckInterval") * 60 * 1000;
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
                _em.Info("Checking Github version");
                
                Release onlineRelease = null;
                if(((string)_plist.Get("Version")).Equals("Latest"))
                {
                    onlineRelease = await _github.GetLatest();

                } else
                {
                    onlineRelease = await _github.GetTarget((string)_plist.Get("Version"));
                }
                
                if(onlineRelease == null)
                {
                    _em.Warning("Can't find online version from Github.");
                    await Task.Delay(_interval, cancellationToken);
                    continue;
                }
                
                Version onlineVersion;
                Version.TryParse(onlineRelease.TagName, out onlineVersion);
                Version agentVersion = _plist.GetAgentVersion();
                if (onlineVersion == agentVersion || onlineVersion < agentVersion)
                {
                    _em.Info("GLPI Agent already up to date");
                    await Task.Delay(_interval, cancellationToken);
                    continue;
                }
                
                _em.Info($"New GLPI Agent available. Current installed version : {agentVersion}. Available version : {onlineVersion}");
                try
                {
                    string filePath = await _github.DownloadRelease(onlineRelease, Path.GetTempPath());
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
    }
}

