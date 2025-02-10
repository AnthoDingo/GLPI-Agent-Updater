using GLPIAgentUpdater.Enums;
using GLPIAgentUpdater.Interfaces;
using GLPIAgentUpdater.Interfaces.Windows;
using GLPIAgentUpdater.Models;
using GLPIAgentUpdater.Services.Shared;

namespace GLPIAgentUpdater.Services.Windows
{
    internal class SMBSource : IChecker
    {
        private IEventManager _em;
        private readonly IRegistry _registry;
        private readonly SMBService _smb;
        private readonly IInstaller _installer;

        private readonly int _interval;

        private Version _agentVersion;

        public SMBSource(
            IEventManager eventManager,
            IRegistry registry,
            SMBService smbService,
            IInstaller installer
        )
        {
            _em = eventManager;
            _registry = registry;
            _smb = smbService;
            _installer = installer;

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
                string? server = (string)_registry.Get("Server");
                string version = (string)_registry.Get("version");

                if (string.IsNullOrEmpty(server))
                {
                    _em.Error("Server cannot be empty");
                    throw new OperationCanceledException();
                }

                FileVersion smbFile;
                if (version.Equals("Latest"))
                {
                    smbFile = await _smb.GetLatest(server, Extension.msi);
                }
                else
                {
                    smbFile = await _smb.GetTarget(server, Extension.msi, version);
                }

                if (smbFile == null)
                {
                    _em.Warning(@$"Canno't find target version {version} on server {server}");
                    await Task.Delay(_interval, cancellationToken);
                    continue;
                }

                Version agentVersion = _registry.GetAgentVersion();
                
                if (smbFile.Version == agentVersion || smbFile.Version < agentVersion)
                {
                    _em.Info("GLPI Agent already up to date");
                    await Task.Delay(_interval, cancellationToken);
                    continue;
                }
    
                try
                {
                    _em.Info($"Copying file {smbFile.FileName} to {Path.GetTempPath()}");
                    File.Copy(Path.Join(server, smbFile.FileName), Path.Join(Path.GetTempPath(), smbFile.FileName));
                    _em.Info("Copy done");
                }
                catch (IOException ex)
                {
                    _em.Error($@"Failed to copy file to {Path.GetTempPath()}");
                    await Task.Delay(_interval, cancellationToken);
                    continue;
                }
                catch (Exception ex)
                {
                    _em.Error($"Unknow error on copy file : {ex.Message}");
                    await Task.Delay(_interval, cancellationToken);
                    continue;
                }
                
                try
                {
                    _em.Info("Installing new version");
                    await _installer.Install(Path.Join(Path.GetTempPath(), smbFile.FileName));
                    _installer.CleanUp(Path.Join(Path.GetTempPath(), smbFile.FileName));
                }
                catch (Exception ex)
                {
                    _em.Error($"Failed to install version {smbFile.Version}");
                }
                
                await Task.Delay(_interval, cancellationToken);
            }
        }
    }
}
