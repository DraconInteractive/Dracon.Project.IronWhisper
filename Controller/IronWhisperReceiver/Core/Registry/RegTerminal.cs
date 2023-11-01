using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Registry
{
    // An AI processor, that parses commands and manages operations among access points. 
    // Currently tied 1-1 with an AccessPoint until a way of managing 'focus' can be found. 
    public class RegTerminal : RegDevice
    {
        public List<string> AccessPoints;

        [JsonIgnore]
        public List<RegAccessPoint> AccessPointData => RegistryManager.Instance.AccessPoints.Where(x => AccessPoints.Contains(x.ID)).ToList();

        public RegTerminal()
        {
            AccessPoints = new List<string>();
        }
    }
}
