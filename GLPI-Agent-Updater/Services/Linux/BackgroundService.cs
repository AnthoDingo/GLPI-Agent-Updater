
using GLPIAgentUpdater.Enums;
using GLPIAgentUpdater.Interfaces;

namespace GLPIAgentUpdater.Services.Linux
{
    internal class BackgroundService : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly IConfig _config;
        private IServiceProvider _serviceProvider;

        public BackgroundService(IConfig config, IServiceProvider serviceProvider)
        {
            _config = config;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            IChecker? checker = null;
            int mode = !String.IsNullOrEmpty((string)_config.Get("Mode")) ? Convert.ToInt32(_config.Get("Mode")) : 0;
            switch (mode)
            {
                case (int)Mode.Github:
                    break;
                case (int)Mode.SMB:
                    break;
                case (int)Mode.GLPI:
                    break;
            }

            if (checker != null)
            {
                await checker.Run(stoppingToken);
            }
        }
    }
}
