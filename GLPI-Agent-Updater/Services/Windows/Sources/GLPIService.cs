using GLPIAgentUpdater.Interfaces;
using GLPIAgentUpdater.Interfaces.Windows;

namespace GLPIAgentUpdater.Services.Windows
{
    internal class GLPIService : IChecker
    {

        private readonly ILogger<GithubService> _logger;
        private readonly IRegistry _registry;

        private Version _agentVersion;

        public GLPIService(ILogger<GithubService> logger, IRegistry registry)
        {
            _logger = logger;
            _registry = registry;

            _agentVersion = _registry.GetAgentVersion();
        }

        public Task Run(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
