namespace GLPIAgentUpdater.Interfaces
{
    internal interface IChecker
    {
        public Task Run(CancellationToken cancellationToken);
    }
}
