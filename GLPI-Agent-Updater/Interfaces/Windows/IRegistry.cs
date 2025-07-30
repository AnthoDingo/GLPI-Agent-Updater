namespace GLPIAgentUpdater.Interfaces.Windows
{
    public interface IRegistry
    {
        public object? Get(string name);

        public object? Get(string key, string name);

        public Version GetAgentVersion();
    }
}
