using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLPIAgentUpdater.Interfaces
{
    internal interface IEventManager
    {
        public void Info(string message);
        public void Info(string message, int Id);

        public void Error(string message);
        public void Error(string message, int Id);

        public void Warning(string message);
        public void Warning(string message, int Id);

    }
}
