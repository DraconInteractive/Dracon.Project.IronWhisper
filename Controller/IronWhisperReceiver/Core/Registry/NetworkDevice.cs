using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Registry
{
    public class NetworkDevice
    {
        public string Address;
        public string Port;
        public DateTime lastUpdateTime;
        public bool Online => (DateTime.Now - lastUpdateTime).TotalMinutes < 5;

        public void UpdateDetails(NetworkDevice details)
        {
            if (!string.IsNullOrEmpty(details.Address))
            {
                Address = details.Address;
            }
            if (!string.IsNullOrEmpty(details.Port))
            {
                Port = details.Port;
            }
            lastUpdateTime = DateTime.Now;
        }

        public NetworkDevice()
        {

        }
    }
}
