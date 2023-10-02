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
        public Networking.NetworkDevice networkDevice;

        public RegDevice () : base()
        {
            deviceID = "TEMP";
            networkDevice = new Networking.NetworkDevice
            {
                lastUpdateTime = DateTime.Now
            };
        }
    }
}
