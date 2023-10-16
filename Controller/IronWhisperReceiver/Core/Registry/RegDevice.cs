using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Registry
{
    public class RegDevice : RegCore
    {
        public string deviceID;
        public NetworkDevice networkDevice;

        public RegDevice () : base()
        {
            deviceID = "TEMP";
            networkDevice = new NetworkDevice
            {
                lastUpdateTime = DateTime.Now
            };
        }
    }
}
