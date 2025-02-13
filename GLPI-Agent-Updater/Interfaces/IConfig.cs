namespace GLPIAgentUpdater.Interfaces;

public interface IConfig
{
    public object? Get(string name);
    public Version GetAgentVersion();
}