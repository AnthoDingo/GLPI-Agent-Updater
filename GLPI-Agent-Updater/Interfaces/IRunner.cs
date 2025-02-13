namespace GLPIAgentUpdater.Interfaces;

public interface IRunner
{
    public Task Run(CancellationToken cancellationToken);
}