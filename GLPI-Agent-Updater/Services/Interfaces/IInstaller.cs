﻿
namespace GLPIAgentUpdater.Services.Interfaces
{
    internal interface IInstaller
    {
        public Task Install(string filePath);

        public void CleanUp(string filePath);
    }
}
