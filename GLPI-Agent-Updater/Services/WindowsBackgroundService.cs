using GLPIAgentUpdater.Enums.Windows;
using GLPIAgentUpdater.Services.Interfaces;
using GLPIAgentUpdater.Services.Windows;

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
            IChecker checker;
            int mode = (int)_registry.Get("Mode");
            switch (mode)
            {
                case (int)Mode.Github:
                    checker = _serviceProvider.GetRequiredService<GithubService>();
                    break;
                case (int)Mode.SMB:

                    break;
                case (int)Mode.GLPI:
                    checker = _serviceProvider.GetRequiredService<GLPIService>();
                    break;
            }


            bool useGithub = (int)_registry.Get("Github") != 0;

            if (useGithub)
            {
                checker = _serviceProvider.GetRequiredService<GithubService>();
            }
            else
            {
                checker = _serviceProvider.GetRequiredService<GLPIService>();
            }

            await checker.Run(stoppingToken);
        }
    }
}
