using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLPIAgentUpdater.Models.Linux
{
    internal class Package
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Architecture { get; set; }
        public string Description { get; set; }
    }
}
