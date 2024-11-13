using GLPIAgentUpdater.Enums.Windows;
using GLPIAgentUpdater.Services.Interfaces;
using GLPIAgentUpdater.Services.Windows;
using System.Diagnostics;

namespace GLPIAgentUpdater.Services
{
    public class WindowsBackgroundService : BackgroundService
    {
        private readonly ILogger<WindowsBackgroundService> _logger;
        private readonly IRegistry _registry;
        private IServiceProvider _serviceProvider;

        public WindowsBackgroundService(
            ILogger<WindowsBackgroundService> logger,
            IRegistry registry,
            IServiceProvider serviceProvider
        )
        {
            _logger = logger;
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
