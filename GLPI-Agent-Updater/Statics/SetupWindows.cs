using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;

namespace GLPIAgentUpdater.Statics
{
    internal static class SetupWindows
    {
        public static void Install(string ServiceName)
        {
            Process install = new Process()
            {
                StartInfo =
                                {
                                    FileName = "sc.exe",
                                    Arguments = $"create \"{ServiceName}\" binPath=\"{System.Environment.ProcessPath}\" start=auto"
                                }
            };
            install.Start();
            install.WaitForExit();

            ServiceController service = new ServiceController(ServiceName);
            if ((service.Status != ServiceControllerStatus.Running) && (service.Status != ServiceControllerStatus.StartPending))
            {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running);
            }
        }

        public static void Uninstall(string ServiceName)
        {
            ServiceController service = new ServiceController(ServiceName);
            if ((service.Status != ServiceControllerStatus.Stopped) && (service.Status != ServiceControllerStatus.StopPending))
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped);
            }

            Process uninstall = new Process()
            {
                StartInfo =
                                {
                                    FileName = "sc.exe",
                                    Arguments = $"delete \"{ServiceName}\""
                                }
            };
            uninstall.Start();
            uninstall.WaitForExit();
        }
    }
}
