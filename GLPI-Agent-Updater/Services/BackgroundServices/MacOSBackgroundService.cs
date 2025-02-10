using GLPIAgentUpdater.Enums;
using GLPIAgentUpdater.Interfaces;
using GLPIAgentUpdater.Interfaces.MacOS;
using GLPIAgentUpdater.Services.MacOS;

namespace GLPIAgentUpdater.Services.BackgroundServices
{
    public class MacOSBackgroundService : BackgroundService
    {
        private readonly IPlist _plist;
        private IServiceProvider _serviceProvider;

        public MacOSBackgroundService(
            IPlist plist,
            IServiceProvider serviceProvider
        )
        {
            _plist = plist;
            _serviceProvider = serviceProvider;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IChecker? checker = null;
            int mode = (int)_plist.Get("Mode");
            switch (mode)
            {
                case (int)Mode.Github:
                    checker = _serviceProvider.GetRequiredService<GithubSource>();
                    break;
                case (int)Mode.SMB :
                    throw new NotImplementedException();
                    // checker = _serviceProvider.GetRequiredService<SMBSource>();
                    break;
                case (int)Mode.GLPI:
                    throw new NotImplementedException();
                    break;
            }

            if (checker != null)
            {
                await checker.Run(stoppingToken);
            }
        }
    }   
}