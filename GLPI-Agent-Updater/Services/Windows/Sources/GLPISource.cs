using GLPIAgentUpdater.Interfaces;
using GLPIAgentUpdater.Interfaces.Windows;

namespace GLPIAgentUpdater.Services.Windows
{
    internal class GLPISource : IChecker
    {

        private readonly ILogger<GithubSource> _logger;
        private readonly IRegistry _registry;

        private Version _agentVersion;

        public GLPISource(ILogger<GithubSource> logger, IRegistry registry)
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
