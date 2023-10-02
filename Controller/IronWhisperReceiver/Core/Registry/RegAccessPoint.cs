using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Registry
{
    // An object capable of running operations
    // E.g, a computer, a microprocessor or a mobile device
    public class RegAccessPoint : RegDevice
    {
        public string Owner;
        public List<string> Projects = new List<string>();
        public List<RegFile> Files = new List<RegFile>();

        public RegAccessPoint()
        {
            Owner = "No Owner";
            Projects = new List<string>();
            Files = new List<RegFile>();
        }
    }
}
