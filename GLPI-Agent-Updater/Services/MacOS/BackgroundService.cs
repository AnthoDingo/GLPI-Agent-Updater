using GLPIAgentUpdater.Enums;
using GLPIAgentUpdater.Interfaces;
using GLPIAgentUpdater.Services.Global;

namespace GLPIAgentUpdater.Services.MacOS
{
    public class BackgroundService : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly IConfig _config;
        private IServiceProvider _serviceProvider;

        public BackgroundService(
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