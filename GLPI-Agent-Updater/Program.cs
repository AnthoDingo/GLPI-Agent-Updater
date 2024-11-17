using GLPIAgentUpdater.Interfaces;
using GLPIAgentUpdater.Interfaces.Windows;
using GLPIAgentUpdater.Services.BackgroundServices;

#if OS_WINDOWS
using GLPIAgentUpdater.Services.Windows;
using GLPIAgentUpdater.Statics;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
#elif OS_LINUX

#elif OS_MAC

#endif


namespace GLPIAgentUpdater
{
    public class Program
    {
        private static readonly string _serviceName = "GLPI Agent Updater";

        private static HostApplicationBuilder builder;

        public static void Main(string[] args)
        {            
            if (args.Length > 0)
            {
                try
                {
                    switch(args[0].ToLower())
                    {
                        case "/install":
                        case "--install":
                            switch (true)
                            {
                                case true when OperatingSystem.IsWindows():
                                    SetupWindows.Install(_serviceName);
                                    break;
                                case true when OperatingSystem.IsLinux():
                                    break;
                                case true when OperatingSystem.IsMacOS():
                                    break;

                            }

                            break;
                        case "/uninstall":
                        case "--uninstall":
                            switch (true)
                            {
                                case true when OperatingSystem.IsWindows():
                                    SetupWindows.Uninstall(_serviceName);
                                    break;
                                case true when OperatingSystem.IsLinux():
                                    break;
                                case true when OperatingSystem.IsMacOS():
                                    break;

                            }
                            break;
                    }


                } catch(Exception ex)
                {
                    Environment.Exit(1);
                }

                Environment.Exit(0);
            }

            builder = Host.CreateApplicationBuilder(args);
            IServiceCollection services = builder.Services;



            switch (true)
            {
                case true when OperatingSystem.IsWindows():
                    services
                    .AddHostedService<WindowsBackgroundService>()
                    .AddWindowsService(options =>
                    {
                        options.ServiceName = _serviceName;
                    });

                        LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(services);

                        services
                            .AddSingleton<IEventManager, EventManager>()
                            .AddSingleton<IRegistry, RegistryService>()
                            .AddSingleton<IInstaller, InstallerService>()
                            .AddSingleton<GithubService>()
                            .AddSingleton<SMBService>()
                            .AddSingleton<GLPIService>()
                            ;
                    break;
                case true when OperatingSystem.IsLinux():
                    break;
                case true when OperatingSystem.IsMacOS():
                    break;

            }
            var app = builder.Build();
            app.Run();
        }
    }
}
