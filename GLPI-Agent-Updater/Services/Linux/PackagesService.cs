using GLPIAgentUpdater.Models.Linux;
using System.Diagnostics;

namespace GLPIAgentUpdater.Services.Linux
{
    internal class PackagesService
    {
        public List<Package> GetInstalledAgents()
        {

            string debPath = "/usr/bin/dpkg"; 
            string rpmPath = "/usr/bin/rpm";

            bool isDeb = File.Exists(debPath); 
            bool isRpm = File.Exists(rpmPath);

            if (isDeb)
            {
                return GetInstalledDeb();
            }
            else if (isRpm)
            {
                return GetInstalledRpm();
            }
            else
            {
                return new List<Package>();
            }
        }

        private List<Package> GetInstalledDeb()
        {
            List<Package> packages = new List<Package>();

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"dpkg -l\" glpi-agent*",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            Process process = new Process { StartInfo = startInfo };
            process.Start();

            string output;
            while ((output = process.StandardOutput.ReadLine()) != null)
            {
                var columns = output.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (columns.Length >= 5)
                {
                    packages.Add(new Package
                    {
                        Name = columns[1],
                        Version = columns[2],
                        Architecture = columns[3],
                        Description = string.Join(" ", columns, 4, columns.Length - 4)
                    });
                }
            }

            process.WaitForExit();

            return packages;
        }

        private List<Package> GetInstalledRpm()
        {
            return new List<Package>();
        }
    }
}
