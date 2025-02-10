namespace GLPIAgentUpdater.Interfaces.MacOS
{
    public interface IPlist
    {
        public object? Get(string name);
        public Version GetAgentVersion();
    }
}