using GLPIAgentUpdater.Interfaces;
using System.Diagnostics;

namespace GLPIAgentUpdater.Services.Linux
{
    internal class InstallerService : IInstaller
    {
        public async Task Install(string filePath)
        {
            try
            {
                Console.WriteLine($"Starting installation : {filePath}");
                Process process = new Process()
                {
                    StartInfo =
                    {
                        FileName = "dpkg",
                        Arguments = $"-i {filePath}"
                    }
                };

                process.Start();
                process.WaitForExit();
                
                //TODO Start service

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to install.\r\n{ex.Message}");
            }
        }

        public void CleanUp(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (DirectoryNotFoundException ex)
            {

            }
            catch (IOException ex)
            {
                Console.WriteLine($"Can't delete file {filePath}.\r\nFile is in used.\r\n{ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Can't delete file {filePath}.\r\nPermission denied.\r\n{ex.Message}");
            }
        }

        
    }
}
