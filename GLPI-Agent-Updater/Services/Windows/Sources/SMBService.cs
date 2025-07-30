using GLPIAgentUpdater.Interfaces;
using GLPIAgentUpdater.Interfaces.Windows;
using GLPIAgentUpdater.Models.Windows;
using System.Text.RegularExpressions;

namespace GLPIAgentUpdater.Services.Windows
{
    internal class SMBService : IChecker
    {
        private IEventManager _em;
        private readonly ILogger<SMBService> _logger;
        private readonly IRegistry _registry;
        private readonly IInstaller _installer;

        private readonly int _interval;

        private Version _agentVersion;

        public SMBService(
            IEventManager eventManager,
            ILogger<SMBService> logger,
            IRegistry registry,
            IInstaller installer
        )
        {
            _em = eventManager;
            _logger = logger;
            _registry = registry;
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

                if (!Directory.Exists($@"{server}")) {
                    _em.Error($@"Path {server} canno't be found.");
                }
                IEnumerable<FileInfo> files = new List<FileInfo>();
                IEnumerable<FileVersion> filesVersion = new List<FileVersion>();
                try
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo($@"{server}");
                    files = directoryInfo.EnumerateFiles();
                    if(files.Count() == 0)
                    {
                        _em.Warning($@"Folder {server} is empty");
                        await Task.Delay(_interval, cancellationToken);
                        continue;
                    }

                    filesVersion = GetFiles(files);
                }
                catch (Exception ex) {
                    _em.Error("Failed to list files");
                    throw new OperationCanceledException();
                }

                FileVersion smbFile;
                _agentVersion = _registry.GetAgentVersion();
                if (version.Equals("Latest"))
                {
                    smbFile = filesVersion.OrderBy(f => f.Version).Last();
                } else
                {
                    Version targetVersion;
                    Version.TryParse(version, out targetVersion);
                    smbFile = filesVersion.Where(f => f.Version == targetVersion).FirstOrDefault();
                }

                if (smbFile != null)
                {
                    if (smbFile.Version == _agentVersion || smbFile.Version < _agentVersion)
                    {
                        _em.Info("GLPI Agent already up to date");
                        await Task.Delay(_interval, cancellationToken);
                        continue;
                    }

                    await InstallFromSMBAsync(smbFile);
                }
                else
                {
                    _em.Warning(@$"Canno't find target version {version} on server :");
                }

                await Task.Delay(_interval, cancellationToken);
            }
        }

        private IEnumerable<FileVersion> GetFiles(IEnumerable<FileInfo> files)
        {
            List<FileVersion> result = new List<FileVersion>();
            Regex regex = new Regex(@"GLPI-Agent-(\d+\.\d+(\.\d+)?)-x64\.msi");

            foreach (FileInfo file in files)
            {
                Match match = regex.Match(file.Name);
                if ((match.Success))
                {
                    Version version;
                    Version.TryParse(match.Groups[1].Value, out version);

                    result.Add(new FileVersion()
                    {
                        FileName = file.Name,
                        Version = version
                    });
                }
            }

            return result;

        }

        private async Task InstallFromSMBAsync(FileVersion file)
        {
            string server = (string)_registry.Get("Server");
            try
            {
                File.Copy($@"{server}\{file.FileName}", $@"C:\Windows\Temp\{file.FileName}", true);
            }
            catch (Exception e)
            {
                _em.Error(@"Failed to copy file to C:\Windows\Temp\");
                return;
            }

            await _installer.Install($@"C:\Windows\Temp\{file.FileName}");
            _installer.CleanUp($@"C:\Windows\Temp\{file.FileName}");
        }
    }
}
