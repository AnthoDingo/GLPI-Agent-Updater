using GLPIAgentUpdater.Enums;
using GLPIAgentUpdater.Interfaces;
using GLPIAgentUpdater.Services.Shared;

namespace GLPIAgentUpdater.Services.BackgroundServices
{
    public class MacOSBackgroundService : BackgroundService
    {
        private readonly IConfig _config;
        private IServiceProvider _serviceProvider;

        public MacOSBackgroundService(
            IConfig config,
            IServiceProvider serviceProvider
        )
        {
            _config = config;
            _serviceProvider = serviceProvider;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IChecker? checker = null;
            int mode = (int)_config.Get("Mode");
            switch (mode)
            {
                case (int)Mode.Github:
                    checker = _serviceProvider.GetRequiredService<GithubService>();
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