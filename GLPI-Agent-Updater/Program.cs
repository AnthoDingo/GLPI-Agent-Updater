using GLPIAgentUpdater.Interfaces;
using GLPIAgentUpdater.Interfaces.MacOS;
using GLPIAgentUpdater.Interfaces.Windows;
using GLPIAgentUpdater.Services.BackgroundServices;
using GLPIAgentUpdater.Services.MacOS;
using GLPIAgentUpdater.Services.Shared;
using GLPIAgentUpdater.Statics;

#if OS_WINDOWS
using GLPIAgentUpdater.Services.Windows;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
#endif

#if OS_LINUX

#endif

#if OS_MAC
using GLPIAgentUpdater.Services.MacOS;
using Mono.Unix;
using Mono.Unix.Native;
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
                        case "/i":
                        case "-i":
                            #if OS_WINDOWS
                            SetupWindows.Install(_serviceName);
                            #endif

                            #if OS_LINUX

                            #endif

                            #if OS_MAC
                            SetupMacOS.Install();
                            #endif
                            break;
                        case "/uninstall":
                        case "--uninstall":
                        case "/u":
                        case "-u":
                            #if OS_WINDOWS
                            SetupWindows.Uninstall(_serviceName);
                            #endif

                            #if OS_LINUX

                            #endif

                            #if OS_MAC
                            SetupMacOS.Uninstall();
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

            services
                .AddSingleton<GithubService>()
                .AddSingleton<SMBService>();
            
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
                .AddSingleton<GithubSource>()
                .AddSingleton<SMBSource>();
            #endif

            #if OS_LINUX

            #endif

            #if OS_MAC
            if (Syscall.getuid() != 0)
            {
                throw new Exception("This application must be run as administrator");
            }

            services
                .AddHostedService<MacOSBackgroundService>();
            
            services
                .AddSingleton<IEventManager, LogManager>()
                .AddSingleton<IPlist, PlistService>()
                .AddSingleton<IInstaller, InstallerService>()
                .AddSingleton<IRunner, GithubService>()
                .AddSingleton<SMBSource>();
            
            #endif

            var app = builder.Build();
            app.Run();
        }
    }
}
