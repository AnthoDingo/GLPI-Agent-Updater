using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLPIAgentUpdater.Interfaces.Linux
{
    internal interface IConfig
    {
        public string GetValue(string key);
    }
}
