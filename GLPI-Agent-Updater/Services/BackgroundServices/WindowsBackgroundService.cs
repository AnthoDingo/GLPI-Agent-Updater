using GLPIAgentUpdater.Enums;
using GLPIAgentUpdater.Interfaces;
using GLPIAgentUpdater.Interfaces.Windows;
using GLPIAgentUpdater.Services.Windows;
using System.Diagnostics;

namespace GLPIAgentUpdater.Services.BackgroundServices
{
    public class WindowsBackgroundService : BackgroundService
    {
        private readonly IRegistry _registry;
        private IServiceProvider _serviceProvider;

        public WindowsBackgroundService(
            IRegistry registry,
            IServiceProvider serviceProvider
        )
        {
            _registry = registry;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IChecker? checker = null;
            int mode = (int)_registry.Get("Mode");
            switch (mode)
            {
                case (int)Mode.Github:
                    checker = _serviceProvider.GetRequiredService<GithubService>();
                    break;
                case (int)Mode.SMB:
                    checker = _serviceProvider.GetRequiredService<SMBService>();
                    break;
                case (int)Mode.GLPI:
                    checker = _serviceProvider.GetRequiredService<GLPIService>();
                    break;
            }

            if (checker != null)
            {
                await checker.Run(stoppingToken);
            }


        }
    }
}
