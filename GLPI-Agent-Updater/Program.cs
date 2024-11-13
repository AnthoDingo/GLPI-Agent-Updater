using GLPIAgentUpdater.Services;
using GLPIAgentUpdater.Services.Interfaces;

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
#if OS_WINDOWS
                            SetupWindows.Install(_serviceName);
#elif OS_LINUX

#elif OS_MAC

#endif

                            break;
                        case "/uninstall":
                        case "--uninstall":
#if OS_WINDOWS
                            SetupWindows.Uninstall(_serviceName);
#elif OS_LINUX

#elif OS_MAC

#endif
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


#if OS_WINDOWS
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
#elif OS_LINUX

#elif OS_MAC

#endif


            var app = builder.Build();
            app.Run();
        }
    }
}
