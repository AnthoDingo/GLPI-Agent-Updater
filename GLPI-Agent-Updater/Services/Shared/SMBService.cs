using GLPIAgentUpdater.Enums;
using GLPIAgentUpdater.Interfaces;
using GLPIAgentUpdater.Models;
using System.Text.RegularExpressions;
using DirectoryNotFoundException = System.IO.DirectoryNotFoundException;

namespace GLPIAgentUpdater.Services.Shared
{
    internal class SMBService
    {
        private IEventManager _em;
        
        public SMBService(IEventManager em)
        {
            _em = em;
        }
        
        internal async Task<IEnumerable<FileVersion>> GetAvailableFiles(string path, Extension extension)
        {
            if (!Directory.Exists($"{path}")) {
                _em.Error($@"Path {path} cannot be found.");
                throw new DirectoryNotFoundException();
            }
            
            IEnumerable<FileInfo> files = new List<FileInfo>();
            IEnumerable<FileVersion> filesVersion = new List<FileVersion>();
            
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo($@"{path}");
                files = directoryInfo.EnumerateFiles();
                if(files.Count() == 0)
                {
                    _em.Warning($@"Folder {path} is empty");
                    return filesVersion;
                }
    
                filesVersion = GetFiles(files, extension);
            }
            catch (Exception ex) {
                _em.Error("Failed to list files");
                throw new OperationCanceledException();
            }
            
            return filesVersion;
        }
        
        private IEnumerable<FileVersion> GetFiles(IEnumerable<FileInfo> files, Extension extension)
        {
            List<FileVersion> result = new List<FileVersion>();
            Regex regex = null;
            switch (extension)
            {
                default:
                case Extension.msi:
                    regex = new Regex(@"GLPI-Agent-(\d+\.\d+(\.\d+)?)-x64\.msi"); 
                    break;
                case Extension.pkg:
                    regex = new Regex(@"GLPI-Agent-(\d+\.\d+(\.\d+)?)_x86_x64\.pkg"); 
                    break;
            }
    
            foreach (FileInfo file in files)
            {
                Match match = regex.Match(file.Name);
                if ((match.Success))
                {
                    Version version;
                    Version.TryParse(match.Groups[1].Value, out version);
    
                    result.Add(new FileVersion()
                    {
                        FileName = file.Name,
                        Version = version
                    });
                }
            }
    
            return result;
        }

        internal async Task<FileVersion> GetLatest(string path, Extension extension)
        {
            IEnumerable<FileVersion> smbFiles = await GetAvailableFiles(path, extension);
            return GetLatest(smbFiles);
        }
        internal FileVersion GetLatest(IEnumerable<FileVersion> files)
        {
            return files.OrderBy(f => f.Version).Last();
        }

        internal async Task<FileVersion> GetTarget(string path, Extension extension, string target)
        {
            IEnumerable<FileVersion> smbFiles = await GetAvailableFiles(path, extension);
            return GetTarget(smbFiles, target);
        }
        internal FileVersion GetTarget(IEnumerable<FileVersion> files, string target)
        {
            Version targetVersion;
            Version.TryParse(target, out targetVersion);
            return GetTarget(files, targetVersion);
        }

        internal async Task<FileVersion> GetTarget(string path, Extension extension, Version target)
        {
            IEnumerable<FileVersion> smbFiles = await GetAvailableFiles(path, extension);
            return GetTarget(smbFiles, target);
        }
        internal FileVersion GetTarget(IEnumerable<FileVersion> files, Version target)
        {
            return files.Where(f => f.Version == target).FirstOrDefault();
        }
    }
}