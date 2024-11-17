
using GLPIAgentUpdater.Enums;
using GLPIAgentUpdater.Interfaces;
using GLPIAgentUpdater.Interfaces.Linux;

namespace GLPIAgentUpdater.Services.BackgroundServices
{
    internal class LinuxBackgroundService : BackgroundService
    {
        private readonly IConfig _config;
        private IServiceProvider _serviceProvider;

        public LinuxBackgroundService(IConfig config, IServiceProvider serviceProvider)
        {
            _config = config;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            IChecker? checker = null;
            int mode = !String.IsNullOrEmpty(_config.GetValue("Mode")) ? Convert.ToInt32(_config.GetValue("Mode")) : 0;
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
