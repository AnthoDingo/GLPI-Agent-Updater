namespace GLPIAgentUpdater.Interfaces;

public interface IGithubSource
{
    public Task Run(CancellationToken cancellationToken);
}